using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga.Cruxell
{
    internal class RadioButtonReplica
    {
        private static Dictionary<string, List<RadioButtonReplica>> _groups = new Dictionary<string, List<RadioButtonReplica>>();
        public string GroupName { get; set; }
        public string Name { get; set; }

        //is checked event
        public event EventHandler CheckedChanged;
       
        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (value)
                {
                    // Uncheck all other radio buttons in the same group
                    if (_groups.ContainsKey(GroupName))
                    {
                        foreach (var radioButton in _groups[GroupName].Where(rb => rb != this))
                        {
                            radioButton._checked = false;
                        }
                    }
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
                _checked = value;
            }
        }

        // Constructor only takes GroupName and Name
        public RadioButtonReplica(string groupName, string name)
        {
            GroupName = groupName;
            Name = name;
            _checked = false;

            // Add this radio button to the appropriate group
            if (!_groups.ContainsKey(groupName))
            {
                _groups[groupName] = new List<RadioButtonReplica>();
            }
            _groups[groupName].Add(this);
            CheckedChanged = new EventHandler((sender, e) => { });
        }
    }
}
