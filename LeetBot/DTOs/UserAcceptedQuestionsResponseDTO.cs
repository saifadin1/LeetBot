using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetBot.DTOs
{
    public class UserAcceptedQuestionsResponseDTO
    {
        public List<UserAcceptedQuestionsDTO> NumAcceptedQuestions { get; set; }
        public string LeetCodeUsername { get; set; }
    }
}
