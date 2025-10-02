using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Extensions;
using Newtonsoft.Json;
using System.Data;

namespace JNJServices.Business.Services
{
    public class AssignmentMetricsService : IAssignmentMetricsService
    {
        private readonly IDapperContext _context;
        public AssignmentMetricsService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<List<AssignmentMetricsResponse>> FetchAssignmentMetrics(int ReservationsAssignmentsID)
        {
            var procedureName = ProcEntities.spAssignmentMetricsCalculate;
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, ReservationsAssignmentsID, DbType.Int32);

            var result = await _context.ExecuteQueryAsync<AssignmentMetricsResponse>(procedureName, parameters, CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<List<AssignmentTrackOtherRecordResponseModel>> FetchWaitingRecords(int ReservationsAssignmentsID)
        {
            var query = "Select * From ReservationAssignmentTrackOtherRecords where ReservationsAssignmentsID = @ReservationsAssignmentsID order by WaitingID";
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, ReservationsAssignmentsID, DbType.Int32);

            var recordList = await _context.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(query, parameters, commandType: CommandType.Text);
            return recordList.ToList();
        }

        public async Task InsertAssignmentMetricsFromJsonAsync(List<SaveAssignmentMetricsViewModel> metricsList)
        {
            var procedureName = ProcEntities.spInsertAssignmentMetrics;
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.JsonData, JsonConvert.SerializeObject(metricsList), DbType.String);

            await _context.ExecuteScalerAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> InsertReservationAssignmentTrackRecordMetrics(SaveAssignmentTrackRecordViewModel model)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, model.ContractorID, dbType: DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, dbType: DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentTrackingID, model.ReservationAssignmentTrackingID, dbType: DbType.Int32);
            parameters.Add(DbParams.TimeInterval, model.TimeInterval, dbType: DbType.String);
            parameters.Add(DbParams.Comments, model.Comments, dbType: DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _context.ExecuteQueryAsync<dynamic>(ProcEntities.spAddAssignmentTrackRecordMetrics, parameters, commandType: CommandType.StoredProcedure);
            int responseCode = parameters.Get<int>(DbParams.ResponseCode);

            return responseCode;
        }

        public async Task<int> DeleteReservationAssignmentTrackRecordMetrics(int waitingId)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.WaitingID, waitingId, dbType: DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _context.ExecuteQueryAsync<dynamic>(ProcEntities.spDeleteAssignmentTrackRecordMetrics, parameters, commandType: CommandType.StoredProcedure);
            int responseCode = parameters.Get<int>(DbParams.ResponseCode);

            return responseCode;
        }

        public async Task<IEnumerable<vwReservationAssignmentsSearch>> MetricsSearch(AssignmentMetricsSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spAssignmentMetricsSearch;
            var parameters = new DynamicParameters();

            if (model.ReservationID.ToValidateIntWithZero())
                parameters.Add(DbParams.ReservationID, model.ReservationID, DbType.Int32);

            if (model.ReservationsAssignmentsID.ToValidateIntWithZero())
                parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);

            if (model.ContractorID.ToValidateIntWithZero())
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.DateFrom))
                parameters.Add(DbParams.DateFrom, Convert.ToDateTime(model.DateFrom), DbType.DateTime);

            if (!string.IsNullOrEmpty(model.DateTo))
                parameters.Add(DbParams.DateTo, Convert.ToDateTime(model.DateTo), DbType.DateTime);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.claimnumber))
                parameters.Add(DbParams.ClaimNumber, model.claimnumber, DbType.String);

            if (model.IsMetricsEntered != null && model.IsMetricsEntered != -2)
                parameters.Add(DbParams.IsMetricsEntered, model.IsMetricsEntered, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page);
            parameters.Add(DbParams.Limit, model.Limit);

            return await _context.ExecuteQueryAsync<vwReservationAssignmentsSearch>(procedureName, parameters, CommandType.StoredProcedure);
        }

        public async Task<List<UploadedFileInfo>> FetchMetricsUploadedDocuments(int ReservationsAssignmentsID)
        {
            var query = @"SELECT ContextType AS 'ContentType',FileName AS 'FileName',DocumentPath AS 'FilePath'  FROM ReservationsAssgnMetricsDocuments 
                  WHERE ReservationsAssignmentsID = @ReservationsAssignmentsID";  // Assuming DocumentID is the primary key

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, ReservationsAssignmentsID, DbType.Int32);

            var documentList = await _context.ExecuteQueryAsync<UploadedFileInfo>(
                query, parameters, commandType: CommandType.Text);

            return documentList.ToList();
        }

        public async Task<IEnumerable<MetricsListResponseModel>> MetricsListSearch(AssignmentMetricsSearchWebViewModel model)
        {
            string procedureName = ProcEntities.spAssignmentMetricsListSearch;
            var parameters = new DynamicParameters();

            if (model.ReservationID.ToValidateIntWithZero())
                parameters.Add(DbParams.ReservationID, model.ReservationID, DbType.Int32);

            if (model.ReservationsAssignmentsID.ToValidateIntWithZero())
                parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);

            if (model.ContractorID.ToValidateIntWithZero())
                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.DateFrom))
                parameters.Add(DbParams.DateFrom, Convert.ToDateTime(model.DateFrom), DbType.DateTime);

            if (!string.IsNullOrEmpty(model.DateTo))
                parameters.Add(DbParams.DateTo, Convert.ToDateTime(model.DateTo), DbType.DateTime);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.claimnumber))
                parameters.Add(DbParams.ClaimNumber, model.claimnumber, DbType.String);

            if (model.IsMetricsEntered != null && model.IsMetricsEntered != -2)
                parameters.Add(DbParams.IsMetricsEntered, model.IsMetricsEntered, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page);
            parameters.Add(DbParams.Limit, model.Limit);

            return await _context.ExecuteQueryAsync<MetricsListResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
        }

    }
}
