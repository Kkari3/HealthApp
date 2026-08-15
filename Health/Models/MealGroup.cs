using System.Collections.Generic;

namespace Health.Models
{
    public class MealGroup : List<MealEntry>
    {
        public string DateLabel { get; set; }

        public MealGroup(string label, IEnumerable<MealEntry> items) : base(items)
        {
            DateLabel = label;
        }
    }
}
