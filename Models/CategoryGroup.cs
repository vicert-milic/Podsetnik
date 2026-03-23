using System.Collections.ObjectModel;

namespace Podsetnik.Models;

public class CategoryGroup : ObservableCollection<PlanItem>
{
    public string Name { get; }

    public CategoryGroup(string name, IEnumerable<PlanItem> items) : base(items)
    {
        Name = name;
    }
}
