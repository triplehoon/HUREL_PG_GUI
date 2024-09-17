using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga.Cruxell
{
    internal class ComboBoxReplica
    {
        public string Name { get; set; }
        private int _selectedIndex;
        public int SelectedIndex 
        { 
            get => _selectedIndex;
            set
            { 
                if (_selectedIndex == value) {
                    return;
                }
                if (value >= 0 && value < Items.Count)
                {
                    _selectedIndex = value;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    // assert false
                    Debug.Assert(false, "Invalid index");
                }
            }
        }
        public List<string> Items { get; set; }        
        public string Text
        {
            get
            {
                return Items[_selectedIndex];
            }
            set
            {
                if (value == null)
                {
                    Debug.Assert(false, "Invalid string");
                    return;
                }
                if (value == Text)
                {
                    return;
                }
                if (Items.Count == 0)
                {
                    Debug.Assert(false, "No items in the list");
                    return;
                }
                // find set string in Items
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == value)
                    {
                        SelectedIndex = i;
                        return;
                    }
                }
                Debug.Assert(false, "Invalid string");
            }
        }
        // events when the selected index changes
        public event EventHandler SelectedIndexChanged;

        public ComboBoxReplica(string name)
        {       
            Name = name;
            Items = new List<string>();
            _selectedIndex = -1;
            SelectedIndexChanged = new EventHandler((sender, e) => { });
        }
    }
}
