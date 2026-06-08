using System.ComponentModel.DataAnnotations;

namespace TraceMap.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "이메일을 입력하세요.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    [Display(Name = "이메일")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호를 입력하세요.")]
    [DataType(DataType.Password)]
    [Display(Name = "비밀번호")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "로그인 유지")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
