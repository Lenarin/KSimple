using System.ComponentModel.DataAnnotations.Schema;

namespace KSimple.Models.Responses
{
    [NotMapped]
    public class ErrorResponse
    {
        public string Error { get; set; }

        public ErrorResponse(string error)
        {
            this.Error = error;
        }
    }
}