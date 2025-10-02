using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JNJServices.Models.Entities.Views
{
    [Table("vwReservationSearch")]
    public class vwReservationSearch
    {
        [Key]
        public int reservationid { get; set; }

        [Key]
        public int claimid { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime reservationtime { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime ReservationDate { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime ReservationTimeSort { get; set; }

        [StringLength(8)]
        public string ReservationTimeF { get; set; } = string.Empty;

        [StringLength(13)]
        public string ReservationTimewZone { get; set; } = string.Empty;

        [StringLength(20)]
        public string RSVPRCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string TRNTYCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string RSVTTCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string RSVSVCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string rsvaccode { get; set; } = string.Empty;

        public int? inactiveflag { get; set; }

        public int? completedflag { get; set; }

        public int? closedflag { get; set; }

        public int? arflag { get; set; }

        public int? metricscompletedflag { get; set; }

        public int? apflag { get; set; }

        [StringLength(500)]
        public string notes { get; set; } = string.Empty;

        [StringLength(20)]
        public string VEHSZCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string RSVRSCode { get; set; } = string.Empty;

        [Key]
        public int TranSummReqFlag { get; set; }

        [StringLength(50)]
        public string ReservationMadeBy { get; set; } = string.Empty;

        public DateTime? createdate { get; set; }

        public DateTime? TranSummRecDate { get; set; }

        [StringLength(50)]
        public string PrescriberName { get; set; } = string.Empty;

        [StringLength(20)]
        public string RSVCXCode { get; set; } = string.Empty;

        public int? pickuplocid { get; set; }

        [StringLength(50)]
        public string PUFacilityName { get; set; } = string.Empty;

        [StringLength(50)]
        public string PUFacilityName2 { get; set; } = string.Empty;

        [StringLength(20)]
        public string PUShortName { get; set; } = string.Empty;

        public int? PUHmMiles { get; set; }

        public int? PUAltMiles { get; set; }

        public int? PUWkMiles { get; set; }

        [StringLength(50)]
        public string PUAddress1 { get; set; } = string.Empty;

        [StringLength(50)]
        public string PUAddress2 { get; set; } = string.Empty;

        [StringLength(50)]
        public string pucity { get; set; } = string.Empty;

        [StringLength(3)]
        public string PUStatecode { get; set; } = string.Empty;

        [StringLength(10)]
        public string PUZipCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string PUComplex { get; set; } = string.Empty;

        [StringLength(255)]
        public string pudirections { get; set; } = string.Empty;

        [StringLength(20)]
        public string pufacpccode { get; set; } = string.Empty;

        [StringLength(20)]
        public string puphone { get; set; } = string.Empty;

        public int? dropofflocid { get; set; }

        [StringLength(50)]
        public string DOFacilityName { get; set; } = string.Empty;

        [StringLength(50)]
        public string DOFacilityName2 { get; set; } = string.Empty;

        [StringLength(20)]
        public string DOShortName { get; set; } = string.Empty;
        public int? DOHmMiles { get; set; }

        public int? DOAltMiles { get; set; }

        public int? DOWkMiles { get; set; }

        [StringLength(50)]
        public string DOAddress1 { get; set; } = string.Empty;

        [StringLength(50)]
        public string DOAddress2 { get; set; } = string.Empty;

        [StringLength(50)]
        public string DOcity { get; set; } = string.Empty;

        [StringLength(3)]
        public string DOStatecode { get; set; } = string.Empty;

        [StringLength(10)]
        public string DOZipCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string DOComplex { get; set; } = string.Empty;

        [StringLength(255)]
        public string dodirections { get; set; } = string.Empty;

        [StringLength(20)]
        public string dofacpccode { get; set; } = string.Empty;

        [StringLength(20)]
        public string DOphone { get; set; } = string.Empty;

        [Key]
        [StringLength(50)]
        public string claimnumber { get; set; } = string.Empty;

        [StringLength(101)]
        public string claimantname { get; set; } = string.Empty;

        [Key]
        public int waittimeapprovedflag { get; set; }

        [Key]
        public int QuoteNeededFlag { get; set; }

        [StringLength(100)]
        public string cancelreason { get; set; } = string.Empty;
        public int claimantid { get; set; }
        public int customerid { get; set; }
        public int RelatedReservationId { get; set; }
        public int RelatedReservationIdCount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ClaimEmployer { get; set; } = string.Empty;
        public string ClaimEmployerAddress1 { get; set; } = string.Empty;
        public string ClaimEmployerCity { get; set; } = string.Empty;
        public string ClaimEmployerState { get; set; } = string.Empty;
        public string TransType { get; set; } = string.Empty;
        public string VehicleSize { get; set; } = string.Empty;
        public string TripType { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string CancelConfirm { get; set; } = string.Empty;
        public string CanceledBy { get; set; } = string.Empty;
        public DateTime CanceledDate { get; set; }
        public DateTime CanceledTime { get; set; }
        public int CreateBillingRecordFlag { get; set; }
        public int CreateInvoicingRecordFlag { get; set; }
        public int ReservationStatusFlag { get; set; }
        public int RoundTripFlag { get; set; }
        public int archiveflag { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string? CustomerFL { get; set; }
        public string RSVCXdescription { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }
}
