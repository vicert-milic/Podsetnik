using System.Text.Json;
using Podsetnik.Models;

namespace Podsetnik.Services;

public class PlannerService
{
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "plan_items.json");
    private static readonly string CategoriesPath = Path.Combine(FileSystem.AppDataDirectory, "categories.json");
    private List<PlanItem> _items = new();
    private List<string> _categories = new() { "Registracija", "Ostalo" };
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
        if (File.Exists(CategoriesPath))
        {
            var json = File.ReadAllText(CategoriesPath);
            var loaded = JsonSerializer.Deserialize<List<string>>(json);
            if (loaded != null && loaded.Count > 0)
                _categories = loaded;
        }
    }

    private void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(_items));
        File.WriteAllText(CategoriesPath, JsonSerializer.Serialize(_categories));
    }

    public List<PlanItem> GetAll() => _items.OrderBy(i => i.AlarmTime).ToList();

    public List<CategoryGroup> GetGrouped()
    {
        return _items
            .GroupBy(i => i.Category)
            .OrderBy(g => g.Key)
            .Select(g => new CategoryGroup(g.Key, g.OrderBy(i => i.AlarmTime)))
            .ToList();
    }

    public List<string> GetCategories() => _categories.ToList();

    public void AddCategory(string name)
    {
        if (!_categories.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            _categories.Add(name);
            Save();
        }
    }

    public PlanItem Add(string description, DateTime alarmTime, string category)
    {
        var item = new PlanItem
        {
            Id = _nextId++,
            Description = description,
            AlarmTime = alarmTime,
            Category = category
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
