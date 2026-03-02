using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using PopUpUI.Containers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PopUpUI.Elements
{
    public partial class ScrollableList
    {
        private class ListOperations : IDisposable
        {
            private readonly ScrollableList scrollableList;
            private RenderTarget2D texture;

            private PopUpElementCollection Nodes => scrollableList.Nodes;

            public ListOperations(ScrollableList parent)
            {
                scrollableList = parent;
                texture = DrawHelper.CreateRenderTarget(parent.area.Width, parent.area.Height);

                parent.areaUIElement.Texture = texture;
            }

            ~ListOperations()
            {
                Dispose();
            }

            public void Dispose()
            {
                texture.Dispose();
                GC.SuppressFinalize(this);
            }

            public void VisibleAreaChanged()
            {
                texture.Dispose();
                texture = DrawHelper.CreateRenderTarget(scrollableList.area.Width, scrollableList.area.Height);
            }

            public void UpdateNodesAndTexture()
            {
                if (scrollableList.IsFrozen) return;

                // if it will always be updated or it already should be updated we don't need to check other options
                if (scrollableList.UpdateOption != ScrollableListUpdateOptions.EveryFrame && !scrollableList.willBeUpdated)
                {
                    return;
                }

                foreach (var node in Nodes)
                {
                    if (node.Visibility == Visibility.Hidden || (node.IsFrozen && node.FreezingOption.HasFlag(FreezingOptions.StopUpdates))) continue;

                    node.UpdateElements();
                }

                Point offset = GetOffsetFromScrolledDistance() * new Point(-1);

                DrawHelper.SetRenderTarget(texture);
                DrawHelper.BeginBatch(scrollableList.drawOptions);

                foreach (var element in Nodes)
                {
                    if (element.Visibility != Visibility.Visible) continue;

                    foreach (var uiElement in element.GetDisplayInfos())
                    {
                        Rectangle uiElementDestinationRect = uiElement.Boundaries.AddToLocation(offset);

                        if (!texture.Bounds.Intersects(uiElementDestinationRect)) continue;

                        DrawHelper.Batch.Draw(uiElement.GetTexture(), uiElementDestinationRect, Color.White * uiElement.Opacity);
                    }
                }

                DrawHelper.EndBatch();

                scrollableList.areaUIElement.Texture = texture;
                scrollableList.willBeUpdated = false;
            }

            public bool Held(PopUpWindowEventArgs e)
            {
                e = e with { MousePoint = e.MousePoint + GetOffsetFromScrolledDistance() - scrollableList.area.Location };

                IEnumerable<IHoldable> holdableElements = Nodes.WhereWithType<IHoldable>(element =>
                {
                    return element.Visibility != Visibility.Hidden && element is IHoldable holdable &&

                          (holdable.CanBeHeld(e.MousePoint) || holdable.IsHeld) &&
                          !holdable.DoNotHoldIf(element);
                });

                IHoldable heldElement = holdableElements.Where(holdable => holdable.IsHeld).FirstOrDefault() ?? holdableElements.FirstOrDefault();

                heldElement?.HoldStatusUpdate(e, heldElement.ButtonsToHold.HasAnyFlagFrom(e.PressedButtons));

                if (heldElement != null) scrollableList.CheckForUpdateCondition(ScrollableListUpdateOptions.AnyNodeHadEvent);

                return heldElement != null;
            }

            public void HoverStateChange(PopUpWindowEventArgs e, bool isHovered)
            {
                if (!isHovered)
                {
                    e = e with { MousePoint = e.MousePoint + GetOffsetFromScrolledDistance() - scrollableList.area.Location };

                    var hoveredElement = Nodes.FindLast(element => element is IHoverable hoverable && hoverable.IsHovered);
                    (hoveredElement as IHoverable)?.OnHoverStateChange(e, false);
                }

                scrollableList.CheckForUpdateCondition(ScrollableListUpdateOptions.AnyNodeHadEvent);
            }

            public void Hovering(PopUpWindowEventArgs e)
            {
                e = e with { MousePoint = e.MousePoint + GetOffsetFromScrolledDistance() - scrollableList.area.Location };

                IEnumerable<IHoverable> hoverableElements = Nodes.WhereWithType<IHoverable>(element =>
                {
                    return element.Visibility != Visibility.Hidden && element is IHoverable hoverable &&

                          !hoverable.DoNotHoverIf(element);
                });

                if (!hoverableElements.Any()) return;

                bool endAllHovers = false;
                foreach (var element in hoverableElements)
                {
                    if (endAllHovers)
                    {
                        element.OnHoverStateChange(e, false);
                        continue;
                    }

                    if (element.CanBeHovered(e.MousePoint))
                    {
                        if (!element.IsHovered) element.OnHoverStateChange(e, true);

                        endAllHovers = true;
                    }
                    else if (element.IsHovered)
                    {
                        element.OnHoverStateChange(e, false);
                    }

                    if (element.IsHovered) element.OnHovering(e);
                }

                scrollableList.CheckForUpdateCondition(ScrollableListUpdateOptions.AnyNodeHadEvent);
            }

            public void Selected(PopUpWindowEventArgs e)
            {
                e = e with { MousePoint = e.MousePoint + GetOffsetFromScrolledDistance() - scrollableList.area.Location };

                ISelectable selectedElement = (ISelectable)Nodes.FindLast(element =>
                {
                    return element.Visibility != Visibility.Hidden && element is ISelectable selectable &&

                           selectable.CanBeSelected(e.MousePoint) &&
                          !selectable.DoNotSelectIf(element) &&
                           selectable.ButtonsToSelect.HasAnyFlagFrom(e.PressedButtons);
                });

                selectedElement?.OnSelect(e);

                if (selectedElement != null) scrollableList.CheckForUpdateCondition(ScrollableListUpdateOptions.AnyNodeHadEvent);
            }

            public void CheckForOutsideSelected(PopUpWindowEventArgs e)
            {
                e = e with { MousePoint = e.MousePoint + GetOffsetFromScrolledDistance() - scrollableList.area.Location };

                IEnumerable<ISelectable> outsideSelectables = Nodes.WhereWithType<ISelectable>(element =>
                {
                    return element.Visibility != Visibility.Hidden && (element as ISelectable).EnabledOutsideSelecting;
                });

                foreach (var selectable in outsideSelectables)
                {
                    if (!selectable.CanBeSelected(e.MousePoint))
                    {
                        selectable.OnSelectingOutside(e);

                        scrollableList.CheckForUpdateCondition(ScrollableListUpdateOptions.AnyNodeHadEvent);
                    }
                }
            }

            private Point GetOffsetFromScrolledDistance()
            {
                return scrollableList.ScrollsVertically
                            ? new Point(0, (int)scrollableList.scrolledDistance)
                            : new Point((int)scrollableList.scrolledDistance, 0);
            }
        }
    }
}
