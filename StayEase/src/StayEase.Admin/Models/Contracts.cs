namespace StayEase.Admin.Models;
public sealed class AuthResult { public string Token {get;set;}="";public string Name {get;set;}=""; }
public sealed class DashboardDto
{
    public int TotalRooms {get;set;}
    public int RoomTypes {get;set;}
    public int Customers {get;set;}
    public int TotalBookings {get;set;}
    public int Pending {get;set;}
    public int Confirmed {get;set;}
    public int CheckedIn {get;set;}
    public decimal Revenue30Days {get;set;}
    public List<RecentBooking> Recent {get;set;}=[];
}
public sealed class RecentBooking
{
    public string Reference {get;set;}="";
    public string GuestName {get;set;}="";
    public decimal TotalPrice {get;set;}
    public string Status {get;set;}="";
    public DateTime CheckIn {get;set;}
    public DateTime CheckOut {get;set;}
}
public sealed class RoomDto
{
    public int Id {get;set;}
    public string Name {get;set;}="";
    public string Slug {get;set;}="";
    public string Description {get;set;}="";
    public string ImagePath {get;set;}="";
    public string Features {get;set;}="";
    public int AreaSqm {get;set;}
    public int Adults {get;set;}
    public int Children {get;set;}
    public decimal PricePerNight {get;set;}
    public int InitialUnits {get;set;}
    public int UnitCount {get;set;}
    public int ActiveUnits {get;set;}
    public bool IsActive {get;set;}=true;
}
public sealed class BookingDto
{
    public int Id {get;set;}
    public string Reference {get;set;}="";
    public string GuestName {get;set;}="";
    public string GuestEmail {get;set;}="";
    public string GuestPhone {get;set;}="";
    public string Room {get;set;}="";
    public string Unit {get;set;}="";
    public DateTime CheckIn {get;set;}
    public DateTime CheckOut {get;set;}
    public int CheckInHour {get;set;}=14;
    public int CheckOutHour {get;set;}=12;
    public decimal TotalPrice {get;set;}
    public string Status {get;set;}="";
    public DateTime CreatedAtUtc {get;set;}
    public string Note {get;set;}="";
}
public sealed class CustomerDto
{
    public string Id {get;set;}="";
    public string FullName {get;set;}="";
    public string Email {get;set;}="";
    public string? PhoneNumber {get;set;}
    public DateTime CreatedAtUtc {get;set;}
    public int BookingCount {get;set;}
}
public sealed class PaymentDto
{
    public int Id {get;set;}
    public int BookingId {get;set;}
    public string Reference {get;set;}="";
    public string GuestName {get;set;}="";
    public decimal Amount {get;set;}
    public string Status {get;set;}="";
    public string Method {get;set;}="";
    public string PaymentMode {get;set;}="Deposit";
    public DateTime? PaidAtUtc {get;set;}
}
public sealed class ReportDto
{
    public DateTime Date {get;set;}
    public int Bookings {get;set;}
    public int Confirmed {get;set;}
    public decimal GrossAmount {get;set;}
}
public sealed class AdminUserDto
{
    public string Id {get;set;}="";
    public string FullName {get;set;}="";
    public string Email {get;set;}="";
    public string? PhoneNumber {get;set;}
    public bool IsAdmin {get;set;}
}

public sealed class DepositSettingsDto
{
    public int DepositPercent {get;set;}=50;
}
public sealed class RefundDto
{
    public int Id {get;set;}
    public string Reference {get;set;}="";
    public string ContactName {get;set;}="";
    public string ContactEmail {get;set;}="";
    public string ContactPhone {get;set;}="";
    public string BankName {get;set;}="";
    public string BankAccountHolder {get;set;}="";
    public string BankAccountNumber {get;set;}="";
    public decimal Amount {get;set;}
    public string Status {get;set;}="";
    public DateTime RequestedAtUtc {get;set;}
    public DateTime? ProcessedAtUtc {get;set;}
}
