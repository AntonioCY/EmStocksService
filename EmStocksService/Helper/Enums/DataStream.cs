using System.ComponentModel.DataAnnotations;

namespace EmStocksService.Helper.Enums;

public enum DataStream
{
    [Display(Name = "PrimarySource")]
    PrimarySource,
    [Display(Name = "Source 2")]
    SecondarySource,
    [Display(Name = "Source 3")]
    ThirdSource
}
