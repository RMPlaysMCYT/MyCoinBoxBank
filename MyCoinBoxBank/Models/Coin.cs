using System;
using SQLite;

namespace MyCoinBoxBank.Models;

[Table("Coins")]
public class Coin
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Denomination { get; set; } =  string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
    public DateTime LastUpdated { get; set; }
}