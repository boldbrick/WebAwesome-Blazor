#if NET11_0_OR_GREATER
using System;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// The minimum or maximum of a <see cref="WaInput"/> (its min and max attributes, which the CEM types
/// <c>number | string</c> and the element passes unchanged to its native input): a number for a number input, a date,
/// a time or a local date and time for the date and time types, or the raw attribute text.
/// </summary>
/// <remarks>
/// <para>
/// A C# 15 union type, available on net11.0 only; on net9.0 and net10.0 <see cref="WaInput"/> has a string bound plus
/// one strongly typed parameter per value type instead. Every case converts implicitly, so a typed value needs no
/// cast: <c>Min="@(new DateOnly(2026, 1, 1))"</c>, <c>Min="@(2.5m)"</c>, <c>Min="@(10)"</c>. A plain attribute
/// value is the raw text: <c>Min="2026-01-01"</c> and <c>Min="10"</c> render unchanged, and so does a C# suffix
/// (<c>Min="2.5m"</c> renders <c>2.5m</c>; write <c>Min="2.5"</c> or <c>Min="@(2.5m)"</c>).
/// </para>
/// <para>
/// Each case renders the form the native input reads, culture-free:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="string"/>: the text unchanged, for a format no other case covers</description></item>
/// <item><description><see cref="decimal"/>, <see cref="int"/>, <see cref="long"/>, <see cref="ulong"/>: the number in the invariant
/// culture (<c>2.5</c>, <c>-10</c>)</description></item>
/// <item><description><see cref="DateOnly"/>: ISO <c>yyyy-MM-dd</c>, for <see cref="WaInputType.Date"/></description></item>
/// <item><description><see cref="TimeOnly"/>: 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when it has seconds, for
/// <see cref="WaInputType.Time"/></description></item>
/// <item><description><see cref="DateTime"/>: the local date and time <c>yyyy-MM-ddTHH:mm</c>, with seconds and
/// milliseconds when not zero (the kind is ignored), for <see cref="WaInputType.DateTimeLocal"/></description></item>
/// </list>
/// <para>
/// There is no floating-point case: the element converts the bound with <c>Number()</c> and has no notion of a
/// floating or fixed point, so a fraction is a <see cref="decimal"/>. An integer constant such as <c>10</c> is an
/// <see cref="int"/>, as are <see cref="short"/> and <see cref="sbyte"/> values. A <see cref="uint"/>,
/// <see cref="ushort"/>, <see cref="byte"/> or <see cref="char"/> value is ambiguous between the number cases and does
/// not compile, and so is a constant above the <see cref="int"/> range that fits a <see cref="uint"/>
/// (<c>3_000_000_000</c>): cast it or write the suffix (<c>@((long)count)</c>, <c>3_000_000_000L</c>).
/// A nullable value such as a <c>DateOnly?</c> does not convert; pass
/// <c>@(day is { } d ? d : null)</c>. A default instance, which holds no value, renders no attribute, as null does.
/// </para>
/// </remarks>
public union WaInputBound(string, decimal, int, long, ulong, DateOnly, TimeOnly, DateTime);
#endif
