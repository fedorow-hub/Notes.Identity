using System.ComponentModel.DataAnnotations;

namespace Notes.Identity.Models;

public class LoginViewModel
{
    [Required]
    public string UserName { get; set; }
    [Required]
    [DataType(DataType.Password)]//тип Password чтобы при вводе данных пароль не отображался
    public string Password { get; set; }
    public string ReturnUrl { get; set; }
}
