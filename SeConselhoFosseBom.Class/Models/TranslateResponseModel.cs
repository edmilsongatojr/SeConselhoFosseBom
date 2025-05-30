namespace SeConselhoFosseBom.Class.Models
{
    public class TranslateResponseModel
    {
        public int ResponseStatus { get; set; }
        public ICollection<MatchModel>? Matches { get; set; }

        public class MatchModel
        {
            public string? Translation { get; set; }
            public decimal Match { get; set; }
            public decimal? Penalty { get; set; }
        }


    }
}
