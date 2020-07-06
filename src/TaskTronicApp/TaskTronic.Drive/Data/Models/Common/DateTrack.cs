namespace TaskTronic.Drive.Data.Models.Common
{
    using System;

    public abstract class DateTrack
    {
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
