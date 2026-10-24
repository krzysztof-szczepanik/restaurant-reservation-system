using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.DTOs
{
    public enum FilterValueMode
{
    Exact,
    Range
}

public enum FilterMatchMode
{
    Or,
    And
}

public sealed class ValueFilter<T> where T : struct
{
    public FilterValueMode Mode { get; init; } = FilterValueMode.Exact;
    public T? Exact { get; init; }
    public T? From { get; init; }
    public T? To { get; init; }
}

public sealed class ReservationFilterCriteria
{
    public string? SearchText { get; init; }
    public ValueFilter<int> Guests { get; init; } = new();
    public ValueFilter<DateOnly> Date { get; init; } = new();
    public ValueFilter<TimeOnly> Time { get; init; } = new();
    public IReadOnlyList<string?> Zones { get; init; } = [];
    public FilterMatchMode ZoneMatchMode { get; init; } = FilterMatchMode.Or;
    public IReadOnlyList<int> TableIds { get; init; } = [];
    public FilterMatchMode TableMatchMode { get; init; } = FilterMatchMode.Or;
}
}
