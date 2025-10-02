using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.ApiConstants;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Extensions;
using System.Data;

namespace JNJServices.Business.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IDapperContext _context;
        public ReservationService(IDapperContext context)
        {
            _context = context;
        }


        //I have aaded stored Procedure here 'spMobileReservationSearch' but Now using

        public async Task<IEnumerable<ReservationAppResponseModel>> MobileReservationSearch(ReservationSearchViewModel model)
        {
            if (!model.Page.ToValidateIntWithZero())
            {
                model.Page = DefaultAppSettings.PageSize;
            }

            if (!model.Limit.ToValidateIntWithZero())
            {
                model.Limit = DefaultAppSettings.PageSize;
            }

            var parameters = new DynamicParameters();

            string query = ProcEntities.spMobileReservationSearch;


            if (model.reservationid.ToValidateIntWithZero())
            {

                parameters.Add(DbParams.ReservationID, Convert.ToInt32(model.reservationid), DbType.Int32);
            }
            if (!string.IsNullOrEmpty(model.ActionCode))
            {

                parameters.Add(DbParams.RSVACCode, model.ActionCode.Trim(), DbType.String);
            }
            if (model.IsTodayJobNotStarted == 1)
            {

                parameters.Add(DbParams.IsTodayJobNotStarted, model.IsTodayJobNotStarted, DbType.Boolean);

            }
            if (model.ClaimantID.ToValidateIntWithZero())
            {

                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);
            }
            if (!string.IsNullOrEmpty(model.DateFrom) || !string.IsNullOrEmpty(model.DateTo))
            {
                if (!string.IsNullOrEmpty(model.DateFrom) && !string.IsNullOrEmpty(model.DateTo))
                {

                    parameters.Add(DbParams.DateFrom, Convert.ToDateTime(model.DateFrom).Date, DbType.Date);
                    parameters.Add(DbParams.DateTo, Convert.ToDateTime(model.DateTo).Date, DbType.Date);
                }
                else if (!string.IsNullOrEmpty(model.DateFrom) && string.IsNullOrEmpty(model.DateTo))
                {

                    parameters.Add(DbParams.DateFrom, Convert.ToDateTime(model.DateFrom).Date, DbType.Date);
                }
                else if (string.IsNullOrEmpty(model.DateFrom) && !string.IsNullOrEmpty(model.DateTo))
                {

                    parameters.Add(DbParams.DateTo, Convert.ToDateTime(model.DateTo).Date, DbType.Date);
                }
            }
            if (model.ReservationTimeFrom != null || model.ReservationTimeTo != null)
            {
                if (model.ReservationTimeFrom != null && model.ReservationTimeTo != null)
                {

                    parameters.Add(DbParams.ReservationTimeFrom, model.ReservationTimeFrom, DbType.DateTime);
                    parameters.Add(DbParams.ReservationTimeTo, model.ReservationTimeTo, DbType.DateTime);
                }
                else if (model.ReservationTimeFrom != null && model.ReservationTimeTo == null)
                {

                    parameters.Add(DbParams.ReservationTimeFrom, model.ReservationTimeFrom, DbType.DateTime);
                }
                else if (model.ReservationTimeFrom == null && model.ReservationTimeTo != null)
                {

                    parameters.Add(DbParams.ReservationTimeTo, model.ReservationTimeTo, DbType.DateTime);
                }
            }
            if (!string.IsNullOrEmpty(model.ReservationTextSearch))
            {

                parameters.Add(DbParams.ReservationTextSearch, model.ReservationTextSearch, DbType.String);
            }
            if (!string.IsNullOrEmpty(model.ClaimantConfirmation))
            {

                parameters.Add(DbParams.ClaimantConfirmation, model.ClaimantConfirmation, DbType.String);
            }
            if (model.IsJobComplete.ToValidateIntWithZero() && model.Type == 2)
            {

                parameters.Add(DbParams.IsJobComplete, Convert.ToInt32(model.IsJobComplete), DbType.Int32);
            }
            if (model.inactiveflag.ToValidateInActiveFlag())
            {

                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);
            }

            parameters.Add(DbParams.Page, model.Page);
            parameters.Add(DbParams.Limit, model.Limit);
            parameters.Add(DbParams.Type, model.Type, DbType.Int32);
            var response = await _context.ExecuteQueryAsync<ReservationAppResponseModel>(query, parameters, CommandType.StoredProcedure);

            return response;
        }


        //Get all action code From Here (Like ConfPend, Confirmed etc)
        public async Task<IEnumerable<ReservationActionCode>> ReservationActionCode()
        {
            var query = "Select * From codesRSVAC where inactiveflag = 0 and actype='R' order by code";

            return await _context.ExecuteQueryAsync<ReservationActionCode>(query, CommandType.Text);
        }

        //Get all Services like (Transportation ,Transportation and Interpretation etc)
        public async Task<IEnumerable<ReservationsServices>> ReservationServices()
        {
            var query = "Select * From codesRSVSV where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<ReservationsServices>(query, CommandType.Text);

        }

        //Get all the trip type (Round Trip, One Way etc)
        public async Task<IEnumerable<ReservationTripType>> ReservationTripType()
        {
            var query = "Select * From codesRSVTT where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<ReservationTripType>(query, CommandType.Text);

        }

        //Transport type Like (Ambulatory,Delivery etc)
        public async Task<IEnumerable<ReservationTransportType>> ReservationTransportType()
        {
            var query = "Select * From codesTRNTY where inactiveflag = 0 order by description";

            return await _context.ExecuteQueryAsync<ReservationTransportType>(query, CommandType.Text);

        }

        //Web Resevation Search
        public async Task<IEnumerable<ReservationSearchResponseModel>> ReservationSearch(ReservationSearchWebViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DateFrom))
            {
                model.DateFrom = Convert.ToDateTime(model.DateFrom).ToString("yyyy-MM-dd");
            }
            if (!string.IsNullOrEmpty(model.DateTo))
            {
                model.DateTo = Convert.ToDateTime(model.DateTo).ToString("yyyy-MM-dd");
            }

            string procedureName = ProcEntities.spReservationSearch;

            var parameters = new DynamicParameters();

            if (model.reservationid.ToValidateIntWithZero())
                parameters.Add(DbParams.ReservationID, model.reservationid, DbType.Int32);

            if (model.relatedReservationId.ToValidateIntWithZero())
                parameters.Add(DbParams.RelatedReservationId, model.relatedReservationId, DbType.Int32);

            if (!string.IsNullOrEmpty(model.claimnumber))
                parameters.Add(DbParams.ClaimNumber, model.claimnumber, DbType.String);

            if (model.ClaimID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.ActionCode))
                parameters.Add(DbParams.ActionCode, model.ActionCode, DbType.String);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (model.Contractorid.ToValidateIntWithZero())
                parameters.Add(DbParams.ContractorID, model.Contractorid, DbType.Int32);

            if (!string.IsNullOrEmpty(model.DateFrom))
                parameters.Add(DbParams.DateFrom, Convert.ToDateTime(model.DateFrom), DbType.DateTime);

            if (!string.IsNullOrEmpty(model.DateTo))
                parameters.Add(DbParams.DateTo, Convert.ToDateTime(model.DateTo), DbType.DateTime);

            if (!string.IsNullOrEmpty(model.Service))
                parameters.Add(DbParams.Service, model.Service, DbType.String);

            if (model.inactiveflag.ToValidateInActiveFlag())
                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);

            if (!string.IsNullOrEmpty(model.TransportType))
                parameters.Add(DbParams.TransportType, model.TransportType, DbType.String);

            if (!string.IsNullOrEmpty(model.TripType))
                parameters.Add(DbParams.TripType, model.TripType, DbType.String);

            if (!string.IsNullOrEmpty(model.ClaimantConfirmation))
                parameters.Add(DbParams.ClaimantConfirmation, model.ClaimantConfirmation, DbType.String);

            if (model.ConAssignStatus.ToValidateIntWithZero())
                parameters.Add(DbParams.ConAssignStatus, model.ConAssignStatus, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<ReservationSearchResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            return response;
        }

        public async Task<IEnumerable<ReservationListResponseModel>> ReservationListSearch(ReservationSearchWebViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DateFrom))
            {
                model.DateFrom = Convert.ToDateTime(model.DateFrom).ToString("yyyy-MM-dd");
            }
            if (!string.IsNullOrEmpty(model.DateTo))
            {
                model.DateTo = Convert.ToDateTime(model.DateTo).ToString("yyyy-MM-dd");
            }

            string procedureName = ProcEntities.spReservationListSearch;

            var parameters = new DynamicParameters();

            if (model.reservationid.ToValidateIntWithZero())
                parameters.Add(DbParams.ReservationID, model.reservationid, DbType.Int32);

            if (model.relatedReservationId.ToValidateIntWithZero())
                parameters.Add(DbParams.RelatedReservationId, model.relatedReservationId, DbType.Int32);

            if (!string.IsNullOrEmpty(model.claimnumber))
                parameters.Add(DbParams.ClaimNumber, model.claimnumber, DbType.String);

            if (model.ClaimID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.ActionCode))
                parameters.Add(DbParams.ActionCode, model.ActionCode, DbType.String);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (model.Contractorid.ToValidateIntWithZero())
                parameters.Add(DbParams.ContractorID, model.Contractorid, DbType.Int32);

            if (!string.IsNullOrEmpty(model.DateFrom))
                parameters.Add(DbParams.DateFrom, Convert.ToDateTime(model.DateFrom), DbType.DateTime);

            if (!string.IsNullOrEmpty(model.DateTo))
                parameters.Add(DbParams.DateTo, Convert.ToDateTime(model.DateTo), DbType.DateTime);

            if (!string.IsNullOrEmpty(model.Service))
                parameters.Add(DbParams.Service, model.Service, DbType.String);

            if (model.inactiveflag.ToValidateInActiveFlag())
                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);

            if (!string.IsNullOrEmpty(model.TransportType))
                parameters.Add(DbParams.TransportType, model.TransportType, DbType.String);

            if (!string.IsNullOrEmpty(model.TripType))
                parameters.Add(DbParams.TripType, model.TripType, DbType.String);

            if (!string.IsNullOrEmpty(model.ClaimantConfirmation))
                parameters.Add(DbParams.ClaimantConfirmation, model.ClaimantConfirmation, DbType.String);

            if (model.ConAssignStatus.ToValidateIntWithZero())
                parameters.Add(DbParams.ConAssignStatus, model.ConAssignStatus, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<ReservationListResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            return response;
        }
    }
}
