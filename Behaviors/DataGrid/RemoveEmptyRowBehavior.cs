using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;

namespace Behaviors.DataGrid
{
    // Requires System.Windows.Interactivity
    public class RemoveEmptyRowBehavior<T> : Behavior<System.Windows.Controls.DataGrid>
    {
        private System.Windows.Controls.DataGrid dataGrid;
        private IKeyboardSimulator keyboard = new InputSimulator().Keyboard;
        private List<T> invalidItems = new List<T>();
        private DataGridCellEditEndingEventArgs lastEdit;
        private int placeholderIndex = -1;

        protected override void OnAttached()
        {
            base.OnAttached();

            dataGrid = base.AssociatedObject;
            dataGrid.CellEditEnding += dataGrid_CellEditEnding;
            dataGrid.CurrentCellChanged += dataGrid_CurrentCellChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (dataGrid != null)
            {
                dataGrid.CellEditEnding -= dataGrid_CellEditEnding;
                dataGrid.CurrentCellChanged -= dataGrid_CurrentCellChanged;
            }
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel && e.Row.GetIndex() == placeholderIndex)
            {
                invalidItems.Add((T)e.Row.Item);
            }
            else if (e.EditAction == DataGridEditAction.Commit && e.Cancel)
            {
                dataGrid.CancelEdit();
            }

            lastEdit = e;
        }

        private void dataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGridCellInfo currentCell = dataGrid.CurrentCell;

            if (invalidItems.Count > 0 && currentCell.Item != lastEdit.Row.Item)
            {
                foreach (T item in invalidItems)
                {
                    (dataGrid.ItemsSource as IList<T>).Remove(item);
                }

                invalidItems.Clear();
                RefreshAndRefocus(currentCell);

                if (dataGrid.Items.IndexOf(currentCell.Item) != placeholderIndex)
                {
                    keyboard.KeyPress(VirtualKeyCode.DOWN);
                    keyboard.KeyPress(VirtualKeyCode.UP);
                }
                else if (dataGrid.Columns.Count > 1)
                {
                    if (currentCell.Column.DisplayIndex == 0)
                    {
                        keyboard.KeyPress(VirtualKeyCode.RIGHT);
                        keyboard.KeyPress(VirtualKeyCode.LEFT);
                    }
                    else
                    {
                        keyboard.KeyPress(VirtualKeyCode.LEFT);
                        keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    }
                }

                if (!dataGrid.Items.Contains(lastEdit.Row.Item) && dataGrid.Items.CurrentItem == lastEdit.Row.Item
                    && lastEdit.EditAction == DataGridEditAction.Commit && !lastEdit.Cancel)
                {
                    (dataGrid.ItemsSource as IList<T>).Add((T)lastEdit.Row.Item);
                    RefreshAndRefocus(currentCell);
                }
            }

            if (currentCell.Item == CollectionView.NewItemPlaceholder)
            {
                placeholderIndex = dataGrid.Items.IndexOf(currentCell.Item);
            }
        }

        private void RefreshAndRefocus(DataGridCellInfo cellToBeFocused)
        {
            dataGrid.Items.Refresh();
            dataGrid.ScrollIntoView(cellToBeFocused.Item, cellToBeFocused.Column);
            dataGrid.SelectedItem = cellToBeFocused.Item;
            dataGrid.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            dataGrid.CurrentCell = cellToBeFocused;
        }
    }
}
