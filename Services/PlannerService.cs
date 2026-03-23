using System.Text.Json;
using Podsetnik.Models;

namespace Podsetnik.Services;

public class PlannerService
{
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "plan_items.json");
    private List<PlanItem> _items = new();
    private int _nextId = 1;

    public PlannerService()
    {
        Load();
    }

    private void Load()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            _items = JsonSerializer.Deserialize<List<PlanItem>>(json) ?? new();
            _nextId = _items.Count > 0 ? _items.Max(i => i.Id) + 1 : 1;
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_items);
        File.WriteAllText(FilePath, json);
    }

    public List<PlanItem> GetAll() => _items.OrderBy(i => i.AlarmTime).ToList();

    public PlanItem Add(string description, DateTime alarmTime)
    {
        var item = new PlanItem
        {
            Id = _nextId++,
            Description = description,
            AlarmTime = alarmTime
        };
        _items.Add(item);
        Save();
        return item;
    }

    public void Delete(int id)
    {
        _items.RemoveAll(i => i.Id == id);
        Save();
    }

    public void ToggleComplete(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
            Save();
        }
    }
}
