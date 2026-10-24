using System.ComponentModel.DataAnnotations;

namespace StayEase.Web.ViewModels;

public sealed class RegisterVm
{
    [Required(ErrorMessage="Nhập họ tên"), StringLength(120)] public string FullName {get;set;}="";
    [Required,EmailAddress] public string Email {get;set;}="";
    [Required,Phone] public string Phone {get;set;}="";
    [Required,MinLength(8),DataType(DataType.Password)] public string Password {get;set;}="";
    [Required,Compare(nameof(Password)),DataType(DataType.Password)] public string ConfirmPassword {get;set;}="";
    [Range(typeof(bool),"true","true",ErrorMessage="Bạn cần đồng ý điều khoản")] public bool AcceptTerms {get;set;}
}
public sealed class LoginVm
{
    [Required,EmailAddress] public string Email {get;set;}="";
    [Required,DataType(DataType.Password)] public string Password {get;set;}="";
    public bool RememberMe {get;set;}
}
public sealed class BookingVm
{
    [Required] public int RoomTypeId {get;set;}
    [Required,DataType(DataType.Date)] public DateOnly CheckIn {get;set;}
    [Required,DataType(DataType.Date)] public DateOnly CheckOut {get;set;}
    [Range(0,23)] public int CheckInHour {get;set;}=14;
    [Range(0,23)] public int CheckOutHour {get;set;}=12;
    [Range(1,10)] public int Adults {get;set;}=2;
    [Range(0,10)] public int Children {get;set;}
    [Required,StringLength(120)] public string GuestName {get;set;}="";
    [Required,EmailAddress,StringLength(150)] public string GuestEmail {get;set;}="";
    [Required,Phone,StringLength(25)] public string GuestPhone {get;set;}="";
    [Required(ErrorMessage="Nhập CCCD"), RegularExpression(@"^\d{12}$",ErrorMessage="CCCD phải gồm 12 chữ số")] public string GuestCitizenId {get;set;}="";
    [StringLength(1000)] public string Note {get;set;}="";
}
public sealed class ProfileVm
{
    [Required,StringLength(120)] public string FullName {get;set;}="";
    [Required,EmailAddress] public string Email {get;set;}="";
    [RegularExpression(@"^$|^\d{12}$",ErrorMessage="CCCD phải gồm 12 chữ số")] public string? CitizenId {get;set;}
    public string? CurrentPasswordForEmail {get;set;}
    [Phone] public string? Phone {get;set;}
    [DataType(DataType.Date)] public DateOnly? DateOfBirth {get;set;}
}

public sealed class CancelBookingVm
{
    [Required] public int BookingId {get;set;}
    [Required,StringLength(120)] public string ContactName {get;set;}="";
    [Required,EmailAddress] public string ContactEmail {get;set;}="";
    [Required,Phone] public string ContactPhone {get;set;}="";
    [Required,StringLength(100)] public string BankName {get;set;}="";
    [Required,StringLength(100)] public string BankAccountHolder {get;set;}="";
    [Required,RegularExpression(@"^\d{6,25}$",ErrorMessage="Số tài khoản cần 6–25 chữ số")] public string BankAccountNumber {get;set;}="";
}
public sealed class RescheduleVm
{
    [Required] public int BookingId {get;set;}
    [Required] public DateOnly CheckIn {get;set;}
    [Required] public DateOnly CheckOut {get;set;}
    [Range(0,23)] public int CheckInHour {get;set;}=14;
    [Range(0,23)] public int CheckOutHour {get;set;}=12;
}
