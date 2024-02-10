using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Observer<T>
{
    // Update is called once per frame
    public abstract void ToUpdate(T value);
}

public interface Observed<T>
{
    public List<Observer<T>> observers { get; set; }
    public void AddObserver(Observer<T> observer) { observers.Add(observer); }
    public void RemoveObserver(Observer<T> observer) { observers.Remove(observer); }
    public void ToUpdate(T value) { foreach(var i in observers) { i.ToUpdate(value); } }
}
