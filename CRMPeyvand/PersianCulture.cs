using System;
using System.Collections.Generic;
using System.Globalization;

public class PersianCulture : CultureInfo
{
    private readonly Calendar cal;
    private readonly Calendar[] optionals;

    public PersianCulture()
        : this("fa-IR", true)
    {
    }

    public PersianCulture(string cultureName, bool useUserOverride)
        : base(cultureName, useUserOverride)
    {
        cal = new PersianCalendar();

        var optionalList = new List<Calendar> { cal };
        foreach (var c in base.OptionalCalendars)
        {
            if (c.GetType() != typeof(PersianCalendar))
            {
                optionalList.Add(c);
            }
        }
        optionals = optionalList.ToArray();

        DateTimeFormat.Calendar = cal;
        DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
        DateTimeFormat.LongDatePattern = "dddd d MMMM yyyy - HH:mm:ss";
        DateTimeFormat.AMDesignator = "ق.ظ";
        DateTimeFormat.PMDesignator = "ب.ظ";

        DateTimeFormat.MonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
        DateTimeFormat.MonthGenitiveNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
        DateTimeFormat.AbbreviatedMonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
        DateTimeFormat.AbbreviatedMonthGenitiveNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };

        DateTimeFormat.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
        DateTimeFormat.ShortestDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
        DateTimeFormat.DayNames = new[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
    }

    public override Calendar Calendar => cal;

    public override Calendar[] OptionalCalendars => optionals;
}