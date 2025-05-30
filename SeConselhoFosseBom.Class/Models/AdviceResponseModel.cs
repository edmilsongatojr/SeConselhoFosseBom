namespace SeConselhoFosseBom.Class.Models
{
    public class AdviceResponseModel
    {
        public SlipModel? Slip { get; set; }

        public class SlipModel
        {
            public int Id { get; set; }
            public string? Advice { get; set; }
        }

    }
}
