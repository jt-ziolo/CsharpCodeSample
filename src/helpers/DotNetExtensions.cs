namespace MyGameName;
using System.Collections.Generic;

public static class DotNetExtensions {
  public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull {
    if (dictionary.ContainsKey(key)) {
      dictionary[key] = value;
      return;
    }
    dictionary.Add(key, value);
  }

  public static void SetBasedOnCurrentValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, System.Func<TValue, TValue> setter, TValue initialValue) where TKey : notnull {
    if (dictionary.ContainsKey(key)) {
      dictionary[key] = setter(dictionary[key]);
      return;
    }
    dictionary.Add(key, initialValue);
  }
}
