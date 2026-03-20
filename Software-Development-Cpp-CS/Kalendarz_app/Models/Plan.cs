using System.Collections.Generic;

namespace Kalendarz.Models
{
    // Model Planu (np. plan uczelniany, pracy, korków)
    public class Plan
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        // Kolekcja zajęć powiązana z planem
        public List<Zajecia> Zajecia { get; set; } = new List<Zajecia>();
        // Notatki powiązane z planem
        public List<Note> Notes { get; set; } = new List<Note>();
        // Studenci (uczniowie) dla planu korków
        public List<Student> Students { get; set; } = new List<Student>();
        // Dni kalendarza (dla widoku kalendarza)
        public Dictionary<string, CalendarDayData> CalendarDays { get; set; } = new Dictionary<string, CalendarDayData>();
    }

    // Dane dnia kalendarza do zapisu
    public class CalendarDayData
    {
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = "#FFFFFFFF";
    }
}
