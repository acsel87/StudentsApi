namespace Student.Models
{
    public class ResponseModel<T>
    {
        public T Model { get; set; }
        public bool IsSuccess { get; set; }
        public string OutputMessage { get; set; }
        public string ErrorAction { get; set; }
        public string ErrorLog { get; set; }
    }
}