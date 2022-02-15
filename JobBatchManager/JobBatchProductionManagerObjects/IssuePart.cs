namespace JobBatchManagerObjects.JobBatchProductionManagerObjects
{
    public class IssuePart
    {
        private string _partNum;
        private string _lotNum;
        private bool _trackLots;
        private decimal _qtyPer;
        private string _mtlSeq;
        private decimal _scrapQty;
        private bool _estWIP;
        private string _idLigneProdFrom;

        public string PartNum { get { return _partNum; } set { _partNum = value; } }
        public string LotNum { get { return _lotNum; } set { _lotNum = value; } }
        public string SetTrackLots { set { _trackLots = value.ToString().Equals("1"); } }
        public string SetQtyPer { set { _qtyPer = decimal.Parse(value.ToString()); } }
        public bool TrackLots { get { return _trackLots; } }
        public decimal QtyPer { get { return _qtyPer; } }
        public string MtlSeq { get { return _mtlSeq; } set { _mtlSeq = value; } }
        public string SetScrapQty { set { _scrapQty = decimal.Parse(value.ToString()); } }
        public decimal ScrapQty { get { return _scrapQty; } }
        public string SetEstWIP { set { _estWIP = value.ToString().Equals("1"); } }
        public bool EstWIP { get => _estWIP; }
        public string IdLigneProdFrom { get => _idLigneProdFrom; set => _idLigneProdFrom = value; }

        public IssuePart() { }

        public string SerializeInfoLot()
        {
            return string.Join("~", new string[] { _partNum, _lotNum, _trackLots ? "1" : "0", _qtyPer.ToString(), _mtlSeq, _estWIP ? "1" : "0", _idLigneProdFrom });
        }
    }
}
