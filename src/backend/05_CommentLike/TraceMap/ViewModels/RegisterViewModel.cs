using System.ComponentModel.DataAnnotations;

namespace TraceMap.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "이름을 입력하세요.")]
    [StringLength(50, ErrorMessage = "이름은 50자 이하로 입력하세요.")]
    [Display(Name = "표시 이름")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "이메일을 입력하세요.")]
    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    [Display(Name = "이메일")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호를 입력하세요.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "비밀번호는 최소 6자 이상이어야 합니다.")]
    [DataType(DataType.Password)]
    [Display(Name = "비밀번호")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "비밀번호 확인을 입력하세요.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "비밀번호와 비밀번호 확인이 일치하지 않습니다.")]
    [Display(Name = "비밀번호 확인")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
