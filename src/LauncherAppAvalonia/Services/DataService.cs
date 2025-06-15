using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.Services;

public class DataService
{
    public string UserDataFolder { get; }
    public string ItemsFilePath { get; }
    public string SettingsFilePath { get; }

    private readonly List<LauncherItem> _items = new();
    private readonly AppSettings _settings = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };


    public DataService()
    {
        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        UserDataFolder = Path.Combine(appDataFolder, "LauncherAppAvalonia", "UserData");
        ItemsFilePath = Path.Combine(UserDataFolder, "items.json");
        SettingsFilePath = Path.Combine(UserDataFolder, "settings.json");

        LoadItems();
        LoadSettings();
    }


    #region Items

    public List<LauncherItem> GetItems()
    {
        return [.._items];
    }

    public void SetItems(IEnumerable<LauncherItem>? items)
    {
        _items.Clear();
        if (items != null)
            _items.AddRange(items);
        SaveItems();
    }

    private void LoadItems()
    {
        _items.Clear();

        try
        {
            if (!File.Exists(ItemsFilePath))
                return;

            string json = File.ReadAllText(ItemsFilePath);
            List<LauncherItem>? loadedItems = JsonSerializer.Deserialize<List<LauncherItem>>(json, _jsonSerializerOptions);
            if (loadedItems != null)
                _items.AddRange(loadedItems);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading items: {ex.Message}");
        }
    }

    private void SaveItems()
    {
        try
        {
            Directory.CreateDirectory(UserDataFolder);
            string json = JsonSerializer.Serialize(_items, _jsonSerializerOptions);
            File.WriteAllText(ItemsFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving items: {ex.Message}");
        }
    }

    #endregion


    #region Settings

    public AppSettings GetSettings()
    {
        return new AppSettings(_settings);
    }

    public void SetSettings(AppSettings? settings)
    {
        if (settings != null)
            _settings.Clone(settings);
        else
            _settings.Reset();

        SaveSettings();
    }

    private void LoadSettings()
    {
        _settings.Reset();

        try
        {
            if (!File.Exists(SettingsFilePath))
                return;

            string json = File.ReadAllText(SettingsFilePath);
            AppSettings? loadedSettings = JsonSerializer.Deserialize<AppSettings>(json, _jsonSerializerOptions);
            if (loadedSettings != null)
                _settings.Clone(loadedSettings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(UserDataFolder);
            string json = JsonSerializer.Serialize(_settings, _jsonSerializerOptions);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    #endregion
}