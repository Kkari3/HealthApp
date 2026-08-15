using SQLite;

namespace Health.Models;

[Table("WeightHistory")]
public class WeightEntry
{
    [PrimaryKey, AutoIncrement]
    [Column("Id")]
    public int Id { get; set; }

    [Unique]
    [Column("Date")]
    public DateTime Date { get; set; }
    [Column("Weight")]
    public int Weight { get; set; }
}