using Dapper;
using JNJServices.Business.Abstracts;
using JNJServices.Data;
using JNJServices.Models.ApiResponseModels;
using JNJServices.Models.ApiResponseModels.App;
using JNJServices.Models.ApiResponseModels.Web;
using JNJServices.Models.CommonModels;
using JNJServices.Models.DbResponseModels;
using JNJServices.Models.Entities;
using JNJServices.Models.Entities.Views;
using JNJServices.Models.ViewModels.App;
using JNJServices.Models.ViewModels.Web;
using JNJServices.Utility.DbConstants;
using JNJServices.Utility.Extensions;
using Newtonsoft.Json;
using System.Data;

namespace JNJServices.Business.Services
{
    public class ReservationAssignmentsService : IReservationAssignmentsService
    {
        private readonly IDapperContext _context;
        public ReservationAssignmentsService(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssignmentService>> ReservationAssignmentType()
        {
            var query = "Select * From codesASSGN where inactiveflag = 0  order by description  ";

            return await _context.ExecuteQueryAsync<AssignmentService>(query, CommandType.Text);
        }

        //I have created stored procedure but didn't use this yet 'spMobileReservationAssignmentSearch'
        public async Task<IEnumerable<ReservationAssignmentAppResponseModel>> MobileReservationAssignmentSearch(AppReservationAssignmentSearchViewModel model)
        {
            var parameters = new DynamicParameters();

            if (model.IsContractorPendingflag != 1)
            {
                string query = "spMobileReservationAssignmentSearch";


                if (model.ContractorID.ToValidateIntWithZero())
                {

                    parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
                }
                if (model.ReservationsAssignmentsID.ToValidateIntWithZero())
                {

                    parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);
                }
                if (!string.IsNullOrEmpty(model.ActionCode))
                {

                    parameters.Add(DbParams.RSVACCode, model.ActionCode.Trim(), DbType.String);
                }
                if (model.ReservationID.ToValidateIntWithZero())
                {

                    parameters.Add(DbParams.ReservationID, model.ReservationID, DbType.Int32);
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
                if (!string.IsNullOrEmpty(model.AssignmentTextSearch))
                {

                    parameters.Add(DbParams.AssignmentTextSearch, model.AssignmentTextSearch, DbType.String);
                }
                if (model.ClaimantID.ToValidateIntWithZero())
                {

                    parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);
                }
                if (model.IsContractorMetricsSubmitted != -1 && model.IsContractorMetricsSubmitted != null)
                {

                    parameters.Add(DbParams.ContractorMetricsFlag, model.IsContractorMetricsSubmitted, DbType.Int32);
                }
                if (model.IsTodayJobNotStarted == 1)
                {

                    parameters.Add(DbParams.IsTodayJobNotStarted, model.IsTodayJobNotStarted, DbType.Boolean);
                }
                if ((model.DriverConfirmFlag == 0 || model.DriverConfirmFlag == -1) && model.DriverConfirmFlag != null)
                {

                    parameters.Add(DbParams.DriverConfirmedFlag, model.DriverConfirmFlag, DbType.Int32);
                }
                parameters.Add(DbParams.Type, model.Type, DbType.Int32);
                if (model.IsJobComplete.ToValidateIntWithZero())
                {

                    parameters.Add(DbParams.IsJobComplete, Convert.ToInt32(model.IsJobComplete), DbType.Int32);
                }
                if (model.IsPastJobComplete.ToValidateIntWithZero())
                {

                    parameters.Add(DbParams.IsPastJobComplete, model.IsPastJobComplete, DbType.Int32);
                }
                if (model.inactiveflag.ToValidateInActiveFlag())
                {

                    parameters.Add(DbParams.InactiveFlag, Convert.ToInt32(model.inactiveflag), DbType.Int32);
                }

                parameters.Add(DbParams.Page, model.Page);
                parameters.Add(DbParams.Limit, model.Limit);

                var assignments = await _context.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(query, parameters, commandType: CommandType.StoredProcedure);
                return assignments.ToList();
            }
            else
            {
                string procedureName = ProcEntities.spGetAllContractorJobRequest;

                parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
                parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

                var assignments = await _context.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
                return assignments.ToList();
            }
        }

        public async Task<IEnumerable<ReservationAssignmentAppResponseModel>> ProcessAssignmentsAsync(List<ReservationAssignmentAppResponseModel> assignments)
        {
            var reservationAssignmentIDs = string.Join(",", assignments.Select(a => a.ReservationsAssignmentsID));
            var contractorIDs = string.Join(",", assignments.Where(x => x.contractorid.HasValue).Select(a => a.contractorid));

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationAssignmentIDs, reservationAssignmentIDs, DbType.String);
            parameters.Add(DbParams.ContractorIDs, contractorIDs, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryMultipleAsync(ProcEntities.spProcessMultipleAssignments, parameters, commandType: CommandType.StoredProcedure);

                var feedbackStatus = await result.ReadAsync<FeedbackStatus>();
                var jobStatus = await result.ReadAsync<JobStatus>();
                var contractorVehicles = await result.ReadAsync<vwContractorVehicleSearch>();
                var contractorDrivers = await result.ReadAsync<vwContractorDriversSearch>();
                var contractorProfileImage = await result.ReadAsync<ContractorProfile>();

                foreach (var item in assignments)
                {
                    if (feedbackStatus != null)
                    {
                        var feedback = feedbackStatus.FirstOrDefault(a => a.ReservationAssignmentID == item.ReservationsAssignmentsID);
                        if (feedback != null)
                        {
                            item.IsFeedbackComplete = feedback.IsFeedbackComplete;
                        }
                    }
                    if (jobStatus != null)
                    {
                        var jobstatus = jobStatus.FirstOrDefault(a => a.ReservationAssignmentID == item.ReservationsAssignmentsID);
                        if (jobstatus != null)
                        {
                            item.IsJobStarted = jobstatus.IsJobStarted;
                        }
                    }
                    if (contractorVehicles.Any())
                    {
                        var contractorvehicle = contractorVehicles.FirstOrDefault(a => a.ContractorID == item.contractorid);
                        if (contractorvehicle != null)
                        {
                            List<vwContractorVehicleSearch> vehicleSearches = new List<vwContractorVehicleSearch>();
                            vehicleSearches.Add(contractorvehicle);
                            item.ContractorVehicles = vehicleSearches;
                        }
                    }
                    if (contractorDrivers.Any())
                    {
                        var ContractorDriver = contractorDrivers.FirstOrDefault(a => a.ContractorID == item.contractorid);
                        if (ContractorDriver != null)
                        {
                            List<vwContractorDriversSearch> driversSearches = new List<vwContractorDriversSearch>();
                            driversSearches.Add(ContractorDriver);
                            item.ContractorDrivers = driversSearches;
                        }
                    }
                    if (contractorProfileImage.Any())
                    {
                        var contractorProfile = contractorProfileImage.FirstOrDefault(a => a.ContractorID == item.contractorid);
                        item.ContractorProfileImage = contractorProfile?.ContractorProfileImage ?? string.Empty;
                    }
                }
            }

            return assignments;
        }

        public async Task<(int, string, List<ReservationAssignmentAcceptRejectAppViewModel>)> MobileAcceptRejectAssignment(ReservationAssignmentAcceptRejectAppViewModel assignmentId)
        {
            var procedureName = ProcEntities.spEditAcceptCancelAssignment;
            var parameters = new DynamicParameters();

            if (assignmentId.ReservationID.ToValidateInt())
                parameters.Add(DbParams.ReservationID, assignmentId.ReservationID, DbType.Int32);

            parameters.Add(DbParams.Type, assignmentId.Type, DbType.Int32);
            parameters.Add(DbParams.ContractorID, assignmentId.ContractorID, DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentID, assignmentId.AssignmentID, DbType.Int32);
            parameters.Add(DbParams.ButtonStatus, assignmentId.ButtonStatus, DbType.String);
            parameters.Add(DbParams.Notes, assignmentId.Notes, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            var response = await _context.ExecuteQueryAsync<ReservationAssignmentAcceptRejectAppViewModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            int responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? string.Empty;

            return (responseCode, message, response.ToList());
        }

        public async Task<(int, string)> MobileCancelAfterAccept(UpdateAssignmentStatusViewModel assignment)
        {
            var procedureName = ProcEntities.spEditCancelAssignmentAfterAccept;
            var parameters = new DynamicParameters();

            if (assignment.ReservationID.ToValidateInt())
                parameters.Add(DbParams.ReservationID, assignment.ReservationID, DbType.Int32);

            parameters.Add(DbParams.Type, assignment.Type, DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentID, assignment.AssignmentID, DbType.Int32);
            parameters.Add(DbParams.ButtonStatus, assignment.ButtonStatus, DbType.String);
            parameters.Add(DbParams.CancelReason, assignment.CancelReason, DbType.String);
            parameters.Add(DbParams.CancelBy, assignment.CancelBy, DbType.String);
            parameters.Add(DbParams.CancelDate, assignment.CancelDate, DbType.DateTime);
            parameters.Add(DbParams.CancelTime, assignment.CancelTime, DbType.DateTime);
            parameters.Add(DbParams.Notes, assignment.Notes, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteQueryAsync<int>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            int responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? string.Empty;

            return (responseCode, message);

        }

        public async Task<IEnumerable<TrackAssignmentByIDResponseModel>> TrackAssignmentByID(int assignmentId)
        {
            string query = "Select rat.*,ras.ReservationID,ras.Quantity,ras.EstimatedMinutes,ras.DOFacilityName2,ras.assgnNum,ras.notes,ras.DOFacilityName,ras.RSVATTCode,ras.ReservationDate,ras.ReservationTime,ras.PickupTime,ras.ClaimantName,ras.HmPhone,ras.PUAddress1,ras.PUAddress2,ras.DOAddress1,ras.DOAddress2,ras.assgncode,ras.ResAsgnCode,ras.Language,ras.ClaimantLanguage1,ras.ClaimantLanguage2,ras.Contractor,ras.TransType  From ReservationAssignmentTracking rat join vwReservationAssignSearch ras on rat.ReservationsAssignmentsID = ras.ReservationsAssignmentsID  where rat.ReservationsAssignmentsID = @ReservationAssignmentID ";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationAssignmentID, assignmentId, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(query, parameters, commandType: CommandType.Text);
            return response;
        }

        public async Task<IEnumerable<TrackAssignmentByIDResponseModel>> RetrieveAssignmentAndContractorDetails(List<TrackAssignmentByIDResponseModel> trackAssignments)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, trackAssignments[0].ContractorID, DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, trackAssignments[0].ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.AssignmentTrackingID, trackAssignments[0].AssignmentTrackingID, DbType.Int32);

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryMultipleAsync(ProcEntities.spTrackAssignmentByIDOtherData, parameters, commandType: CommandType.StoredProcedure);

                // Read and process each result set
                var record = await result.ReadAsync<AssignmentTrackOtherRecordResponseModel>();
                var recordList = await result.ReadAsync<AssignmentTrackOtherRecordResponseModel>();
                var contractorVehicles = await result.ReadAsync<vwContractorVehicleSearch>();
                var contractorDrivers = await result.ReadAsync<vwContractorDriversSearch>();

                // Loop through the trackAssignments and assign the data
                foreach (var items in trackAssignments)
                {
                    if (record != null && record.Any())
                    {
                        items.WaitingRecords = record.ToList();
                    }

                    // Assign the second query result (All records)
                    if (recordList != null && recordList.Any())
                    {
                        items.WaitingRecordsList = recordList.Where(a => a.ReservationAssignmentTrackingID == items.AssignmentTrackingID).ToList();
                    }

                    // Assign contractor vehicles (if any)
                    if (contractorVehicles != null && contractorVehicles.Any())
                    {
                        items.ContractorVehicles = contractorVehicles;
                    }

                    // Assign contractor drivers (if any)
                    if (contractorDrivers != null && contractorDrivers.Any())
                    {
                        items.ContractorDrivers = contractorDrivers;
                    }
                }
            }

            return trackAssignments;
        }

        public async Task<IEnumerable<OngoingAssignmentTrackingResponseModel>> OnGoingAssignment(AppTokenDetails claims)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.UserID, claims.UserID, DbType.Int32);
            parameters.Add(DbParams.Type, claims.Type, DbType.Int32);

            using (var connection = _context.CreateConnection())
            {
                using (var result = await connection.QueryMultipleAsync(ProcEntities.spOngoingAssignments, parameters, commandType: CommandType.StoredProcedure))
                {
                    var ongoingAssignments = await result.ReadAsync<OngoingAssignmentTrackingResponseModel>();

                    // Get the related records for the first assignment
                    var waitingRecords = await result.ReadAsync<AssignmentTrackOtherRecordResponseModel>();
                    var waitingRecordsList = await result.ReadAsync<AssignmentTrackOtherRecordResponseModel>();
                    var contractorVehicles = await result.ReadAsync<vwContractorVehicleSearch>();
                    var contractorDrivers = await result.ReadAsync<vwContractorDriversSearch>();

                    // Attach related records to the ongoing assignments
                    foreach (var item in ongoingAssignments)
                    {
                        // Get related records based on ContractorID, AssignmentTrackingID, and ReservationsAssignmentsID
                        item.WaitingRecords = waitingRecords
                            .Where(w => w.ContractorID == item.ContractorID &&
                                        w.ReservationsAssignmentsID == item.ReservationsAssignmentsID)
                            .ToList();

                        item.WaitingRecordsList = waitingRecordsList.Where(w => w.ReservationAssignmentTrackingID == item.AssignmentTrackingID).ToList();

                        item.ContractorVehicles = contractorVehicles
                            .Where(v => v.ContractorID == item.ContractorID)
                            .ToList();

                        item.ContractorDrivers = contractorDrivers
                            .Where(d => d.ContractorID == item.ContractorID)
                            .ToList();
                    }
                    return ongoingAssignments;
                }
            }
        }

        public async Task LiveTraking(SaveLiveTrackingCoordinatesViewModel liveTraking)
        {
            var query = "INSERT INTO LiveTrackingMap (ReservationsAssignmentsID,AssignmentTrackingID, LatitudeLongitude,TrackingDateTime,IsDeadMile)  VALUES (@ReservationsAssignmentsID,@AssignmentTrackingID, @LatitudeLongitude,@DateTime,@isDeadMile);";
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.ReservationsAssignmentsID, liveTraking.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.AssignmentTrackingID, liveTraking.AssignmentTrackingID, DbType.Int32);
            parameters.Add(DbParams.LatitudeLongitude, liveTraking.LatitudeLongitude, DbType.String);
            parameters.Add(DbParams.DateTime, liveTraking.TrackingDateTime, DbType.DateTime);
            parameters.Add(DbParams.IsDeadMile, liveTraking.isDeadMile, DbType.Int32);

            await _context.ExecuteAsync(query, parameters);
        }

        public async Task<List<Coordinate>> GetCoordinatesFromDatabase(TrackingAssignmentIDViewModel model)
        {
            var query = @"
        SELECT LatitudeLongitude
        FROM [LiveTrackingMap]
        WHERE 
            @ReservationsAssignmentsID IS NOT NULL 
            AND ReservationsAssignmentsID = @ReservationsAssignmentsID
            AND (@AssignmentTrackingID IS NULL OR AssignmentTrackingID = @AssignmentTrackingID)
            AND (@isDeadMile IS NULL OR IsDeadMile = @isDeadMile)
        ORDER BY id ASC;";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.AssignmentTrackingID, model.AssignmentTrackingID, DbType.Int32);
            parameters.Add(DbParams.IsDeadMile, model.isDeadMile, DbType.Int32);

            // Execute the query and fetch all rows
            var results = await _context.ExecuteQueryAsync<string>(query, parameters, CommandType.Text);

            var coordinates = new List<Coordinate>();

            // Iterate over each result, deserialize, and add to the list
            foreach (var result in results)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    try
                    {
                        var coordinate = JsonConvert.DeserializeObject<Coordinate>(result);
                        if (coordinate != null)
                        {
                            coordinates.Add(coordinate);
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Log the error if the deserialization fails
                        ex.GetBaseException();
                    }
                }
            }

            return coordinates;
        }


        public async Task<(IEnumerable<AssignmentTrackingResponseModel>, int, string)> AssignmentTracking(ReservationAssignmentTrackingViewModel model)
        {
            var procedureName = ProcEntities.spAddAssignmentTrackingRecords;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.ButtonID, model.ButtonID, DbType.Int32);
            parameters.Add(DbParams.AssignmentTrackingID, model.AssignmentTrackingID, DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentID, model.ReservationAssignmentID, DbType.Int32);
            parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            parameters.Add(DbParams.ButtonStatus, model.ButtonStatus, DbType.String);
            parameters.Add(DbParams.DriverLatitudeLongitude, model.DriverLatitudeLongitude, DbType.String);
            parameters.Add(DbParams.TravelledDateAndTime, model.TravelledDateandTime, DbType.DateTime);
            parameters.Add(DbParams.CreateDate, model.CreateDate, DbType.DateTime);
            parameters.Add(DbParams.CreateUserID, model.CreateUserID, DbType.Int32);
            parameters.Add(DbParams.DeadMiles, model.DeadMiles, DbType.Decimal);
            parameters.Add(DbParams.TravellingMiles, model.TravellingMiles, DbType.Decimal);
            parameters.Add(DbParams.ImageUrl, model.ImageUrl, DbType.String);
            parameters.Add(DbParams.DeadMileImageUrl, model.DeadMileImageUrl, DbType.String);

            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            var response = await _context.ExecuteQueryAsync<AssignmentTrackingResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";
            return (response, responseCode, message);

        }

        public async Task<IEnumerable<AssignmentTrackingResponseModel>> FetchOtherRecordsForTracking(List<AssignmentTrackingResponseModel> trackingRecords)
        {
            var query = "Select * From ReservationAssignmentTrackOtherRecords where ContractorID = @ContractorID and ReservationsAssignmentsID = @ReservationsAssignmentsID order by WaitingID";
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.ContractorID, trackingRecords[0].ContractorID, DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, trackingRecords[0].ReservationsAssignmentsID, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(query, parameters, CommandType.Text);

            foreach (var item in trackingRecords)
            {
                item.WaitingRecordsList = response.Where(r => r.ReservationAssignmentTrackingID == item.AssignmentTrackingID).ToList();
            }

            return trackingRecords;
        }

        public async Task<(int, string, IEnumerable<AssignmentTrackOtherRecordResponseModel>)> AssignmentTrackingOtherRecords(ReservationAssignmentTrackOtherRecordsViewModel model)
        {
            var procedureName = ProcEntities.spAddAssignmentTrackOtherRecords;
            var parameters = new DynamicParameters();

            parameters.Add(DbParams.ButtonID, model.ButtonID, DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentTrackingID, model.ReservationAssignmentTrackingID, DbType.Int32);
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.WaitingID, model.WaitingID, DbType.Int32);
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            parameters.Add(DbParams.DriverLatitudeLongitude, model.DriverLatitudeLongitude, DbType.String);
            parameters.Add(DbParams.TravelledDateAndTime, model.WaitingDateandTime, DbType.DateTime);
            parameters.Add(DbParams.Comments, model.Comments, DbType.String);
            parameters.Add(DbParams.CreatedDate, model.CreatedDate, DbType.DateTime);
            parameters.Add(DbParams.CreatedUserID, model.CreatedUserID, DbType.Int32);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            var response = await _context.ExecuteQueryAsync<AssignmentTrackOtherRecordResponseModel>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            var responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message, response);
        }

        public async Task<IEnumerable<ReservationCancelDetails>> ReservationAssignmentCancelStatus(int type)
        {
            string query = string.Empty;
            if (type == (int)Utility.Enums.Type.Contractor)
            {
                query = "Select * From codesrsvcx a where a.inactiveflag = 0 and a.contractorflag != 0";
            }
            else if (type == (int)Utility.Enums.Type.Claimant)
            {
                query = "Select * from codesrsvcx a where a.inactiveflag = 0  and a.claimantflag != 0 ";
            }

            var result = await _context.ExecuteQueryAsync<ReservationCancelDetails>(query, commandType: CommandType.Text);
            return result;
        }

        public async Task<List<Coordinate>> GetLiveTrackingCoordinates(TrackingAssignmentIDViewModel model)
        {
            var query = @"
            SELECT LatitudeLongitude
            FROM [LiveTrackingMap]
            WHERE 
                @ReservationsAssignmentsID IS NOT NULL 
                AND ReservationsAssignmentsID = @ReservationsAssignmentsID
                AND (@AssignmentTrackingID IS NULL OR AssignmentTrackingID = @AssignmentTrackingID)
                AND (@isDeadMile IS NULL OR IsDeadMile = @isDeadMile)
            ORDER BY id ASC;";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, model.ReservationsAssignmentsID, DbType.Int32);
            parameters.Add(DbParams.AssignmentTrackingID, model.AssignmentTrackingID, DbType.Int32);
            parameters.Add(DbParams.IsDeadMile, model.isDeadMile, DbType.Int32);

            var result = await _context.ExecuteQueryAsync<string>(query, parameters, CommandType.Text);

            return result.Select(coord => JsonConvert.DeserializeObject<Coordinate>(coord))
             .Where(coordinate => coordinate != null)
             .Cast<Coordinate>()
             .ToList();
        }

        public async Task<List<Coordinate>> GetGooglePathCoordinates(int reservationsAssignmentsID)
        {
            var query = "SELECT LatitudeLongitudePath FROM ReservationAssignmentPath WHERE ReservationsAssignmentsID = @ReservationsAssignmentsID";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, reservationsAssignmentsID, DbType.Int32);

            // Retrieve the JSON string
            var serializedPath = await _context.ExecuteQueryFirstOrDefaultAsync<string>(query, parameters, CommandType.Text);

            return serializedPath != null
                    ? JsonConvert.DeserializeObject<List<Coordinate>>(serializedPath) ?? new List<Coordinate>()
                    : new List<Coordinate>();
        }

        public async Task<string> SearchAssignmentPath(int assignmentId)
        {

            var query = "SELECT LatitudeLongitudePath FROM ReservationAssignmentPath WHERE ReservationsAssignmentsID = @ReservationsAssignmentsID";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, assignmentId, DbType.Int32);

            // Retrieve the JSON string directly
            var result = await _context.ExecuteScalerAsync(query, parameters);

            return result ?? string.Empty;

        }

        public async Task SaveAssignmentPath(int assignmentId, string path, bool updateExisting)
        {
            string query = string.Empty;
            if (updateExisting)
            {
                query = @"UPDATE [ReservationAssignmentPath] SET LatitudeLongitudePath = @latlong WHERE ReservationsAssignmentsID = @ReservationsAssignmentsID";
            }
            else
            {
                query = @"INSERT INTO [ReservationAssignmentPath] (ReservationsAssignmentsID, LatitudeLongitudePath, CreateDateTime)VALUES (@ReservationsAssignmentsID, @latlong, GETDATE())";
            }

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, assignmentId, DbType.Int32);
            parameters.Add(DbParams.LatLong, path, DbType.String);

            await _context.ExecuteAsync(query, parameters, CommandType.Text);
        }

        public async Task<(int, string)> UserFeedback(UserFeedbackAppViewModel model)
        {
            string procedureName = ProcEntities.spAddUsersFeedback;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationAssignmentID, model.ReservationAssignmentID, DbType.Int32);
            parameters.Add(DbParams.FromID, model.FromID, DbType.Int32);
            parameters.Add(DbParams.ToID, model.ToID, DbType.Int32);
            parameters.Add(DbParams.Notes, model.Notes, DbType.String);
            parameters.Add(DbParams.Answer1, model.Answer1, DbType.Decimal);
            parameters.Add(DbParams.Answer2, model.Answer2, DbType.Decimal);
            parameters.Add(DbParams.Answer3, model.Answer3, DbType.Decimal);
            parameters.Add(DbParams.Answer4, model.Answer4, DbType.Decimal);
            parameters.Add(DbParams.Answer5, model.Answer5, DbType.Decimal);
            parameters.Add(DbParams.Answer6, model.Answer6, DbType.Decimal);
            parameters.Add(DbParams.CreatedDate, model.CreatedDate, DbType.DateTime);
            parameters.Add(DbParams.CreatedUserID, model.CreatedUserID, DbType.Int32);

            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);

            await _context.ExecuteAsync(procedureName, parameters, CommandType.StoredProcedure);
            int responseCode = parameters.Get<Int32?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message);
        }

        public async Task<IEnumerable<UserFeedbackAppResponseModel>> ViewUserFeedback(AssignmentIDAppViewModel model)
        {
            string query = "Select * from UsersFeedback where ReservationAssignmentID = @ReservationAssignmentID";
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationAssignmentID, model.ReservationAssignmentID, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<UserFeedbackAppResponseModel>(query, parameters, CommandType.Text);

            return response;
        }

        public async Task<IEnumerable<vwReservationAssignmentsSearch>> ReservationAssignmentSearch(ReservationAssignmentWebViewModel model)
        {
            string procedureName = ProcEntities.spReservationAssignmentsSearch;

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

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (model.ClaimID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.ASSGNCode))
                parameters.Add(DbParams.ASSGNCode, model.ASSGNCode, DbType.String);

            if (!string.IsNullOrEmpty(model.RSVACCode))
                parameters.Add(DbParams.RSVACCode, model.RSVACCode, DbType.String);

            if (!string.IsNullOrEmpty(model.AssignmentJobStatus))
                parameters.Add(DbParams.AssignmentJobStatus, model.AssignmentJobStatus, DbType.String);

            if (model.ContractorContactInfo != null && model.ContractorContactInfo.Count > 0)
                parameters.Add(DbParams.ContractorContactInfo, string.Join(",", model.ContractorContactInfo), DbType.String);

            if (model.inactiveflag.ToValidateInActiveFlag())
                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);

            if (model.ConAssignStatus.ToValidateInt())
                parameters.Add(DbParams.ConAssignStatus, model.ConAssignStatus, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);

            return await _context.ExecuteQueryAsync<vwReservationAssignmentsSearch>(procedureName, parameters, CommandType.StoredProcedure);
        }

        //Assign Contractor Reservation by Web
        public async Task<IEnumerable<ReservationAssigncontractorWebResponseModel>> AssignAssignmentContractor(AssignAssignmentContractorWebViewModel model)
        {
            var procedureName = ProcEntities.spAssignContractor;
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationID, model.reservationid, DbType.Int32);
            parameters.Add(DbParams.ReservationAssignmentID, model.reservationassignmentid, DbType.Int32);
            parameters.Add(DbParams.ContractorID, model.contractorid, DbType.Int32);
            parameters.Add(DbParams.Type, model.AssignType, DbType.String);
            parameters.Add(DbParams.ZipCode, model.zipcode, DbType.String);
            parameters.Add(DbParams.Miles, model.miles, DbType.Decimal);
            parameters.Add(DbParams.ContractorName, model.contractorname, DbType.String);
            parameters.Add(DbParams.Company, model.company, DbType.String);
            parameters.Add(DbParams.CellPhone, model.CellPhone, DbType.String);
            parameters.Add(DbParams.ContyCode, model.contycode, DbType.String);
            parameters.Add(DbParams.ConctCode, model.conctcode, DbType.String);
            parameters.Add(DbParams.ConpcCode, model.conpccode, DbType.String);
            parameters.Add(DbParams.Gender, model.gender, DbType.String);
            parameters.Add(DbParams.City, model.city, DbType.String);
            parameters.Add(DbParams.StateCode, model.statecode, DbType.String);
            parameters.Add(DbParams.Cstatus, model.cstatus, DbType.String);
            parameters.Add(DbParams.ConstCode, model.constcode, DbType.String);
            parameters.Add(DbParams.RatePerMiles, model.RatePerMiles, DbType.Decimal);
            parameters.Add(DbParams.RatePerHour, model.RatePerHour, DbType.Decimal);
            parameters.Add(DbParams.Cost, model.Cost, DbType.String);
            parameters.Add(DbParams.IsPreferredContractor, model.IsPreferredContractor, DbType.Int32);

            var result = await _context.ExecuteQueryAsync<ReservationAssigncontractorWebResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
            return result;
        }

        public async Task<IEnumerable<TrackAssignmentByIDResponseModel>> WebTrackAssignmentByID(int assignmentId)
        {
            string query = "Select rat.*,ras.ReservationID,ras.Quantity,ras.EstimatedMinutes,ras.ContractorMetricsFlag,ras.DOFacilityName2,ras.DOFacilityName,ras.RSVATTCode,ras.assgnNum,ras.notes,ras.ReservationDate,ras.ReservationTime,ras.PickupTime,ras.ClaimantName,ras.HmPhone,ras.PUAddress1,ras.PUAddress2,ras.DOAddress1,ras.DOAddress2,ras.assgncode,ras.ResAsgnCode,ras.Language,ras.ClaimantLanguage1,ras.ClaimantLanguage2,ras.Contractor,ras.ForcedJobCompletionNotes,ras.rsvprCode,ras.procedureDescription,ras.AssignmentJobStatus From ReservationAssignmentTracking rat join vwReservationAssignSearch ras on rat.ReservationsAssignmentsID = ras.ReservationsAssignmentsID  where rat.ReservationsAssignmentsID = @ReservationAssignmentID order By rat.AssignmentTrackingID ";

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationAssignmentID, assignmentId, DbType.Int32);

            var response = await _context.ExecuteQueryAsync<TrackAssignmentByIDResponseModel>(query, parameters, CommandType.Text);
            return response;
        }

        public async Task<(int, string, bool)> CheckReservationAssignmentAsync(int reservationAssignmentId, string notes)
        {
            string procedureName = ProcEntities.spCheckReservationAssignment;

            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationsAssignmentsID, reservationAssignmentId, DbType.Int64);
            parameters.Add(DbParams.Notes, notes, DbType.String);
            parameters.Add(DbParams.ResponseCode, 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, "", dbType: DbType.String, direction: ParameterDirection.Output);
            parameters.Add("ShowNotesBox", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            await _context.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);

            int responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";
            bool ShowNotesBox = parameters.Get<bool?>("ShowNotesBox") ?? false;

            return (responseCode, message, ShowNotesBox);
        }

        public async Task<(List<ReservationAssignmentAppResponseModel>, int, string)> GetAllRelatedAssignmentsAsync(int reservationId)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationID, reservationId, DbType.Int32);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            var result = (await _context.ExecuteQueryAsync<ReservationAssignmentAppResponseModel>(
                "spGetAllRelatedAssignments",
                parameters,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "No message available";

            return (result, responseCode, message);
        }

        public async Task<(int ResponseCode, string Message, ReservationCheckTimeSlotResponseModel)> CheckReservationTimeSlotAsync(ReservationCheckTimeSlotViewModel model)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ReservationAssignmentID, model.ReservationAssignmentID, DbType.Int32);
            parameters.Add(DbParams.ReservationID, model.ReservationID, DbType.Int32);
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            parameters.Add(DbParams.ResponseCode, dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add(DbParams.Msg, dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

            var result = await _context.ExecuteQueryFirstOrDefaultAsync<ReservationCheckTimeSlotResponseModel>(
                ProcEntities.spCheckReservationTimeSlot, parameters, CommandType.StoredProcedure
            ) ?? new ReservationCheckTimeSlotResponseModel(); // Handle potential null result

            int responseCode = parameters.Get<int?>(DbParams.ResponseCode) ?? 0;
            string message = parameters.Get<string?>(DbParams.Msg) ?? "";

            return (responseCode, message, result);
        }

        public async Task<bool> SaveAssignmentDocument(int reservationsAssignmentsID, UploadedFileInfo file)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ReservationsAssignmentsID", reservationsAssignmentsID);
            parameters.Add("@ContextType", file.ContentType);
            parameters.Add("@FileName", file.FileName);
            parameters.Add("@DocumentPath", file.FilePath);

            // Output parameters
            parameters.Add("@ResponseCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Msg", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

            await _context.ExecuteAsync("spInsertAssignmentMetricsDocument", parameters, CommandType.StoredProcedure);

            int responseCode = parameters.Get<int>("@ResponseCode");
            string responseMsg = parameters.Get<string>("@Msg");

            if (responseCode != 1)
            {
                Console.WriteLine($"Database error: {responseMsg}");
                return false;
            }

            return true;
        }

        public async Task<AppMilesRecordResponseModel> AssignmentMilesRecord(AppReservationAssignmentSearchViewModel model)
        {
            var parameters = new DynamicParameters();
            parameters.Add(DbParams.ContractorID, model.ContractorID, DbType.Int32);
            parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);
            parameters.Add(DbParams.UserType, model.Type, DbType.Int32);
            parameters.Add(DbParams.DateFrom, model.DateFrom, DbType.Date);
            parameters.Add(DbParams.DateTo, model.DateTo, DbType.Date);
            parameters.Add(DbParams.IsMetricsEntered, model.IsContractorMetricsSubmitted, DbType.Int32);
            parameters.Add(DbParams.IsPastJobComplete, model.IsPastJobComplete, DbType.Int32);
            var result = await _context.ExecuteQueryFirstOrDefaultAsync<AppMilesRecordResponseModel>(
                ProcEntities.spGetAssignmentTotalMiles, parameters, CommandType.StoredProcedure
            );
            return result ?? new AppMilesRecordResponseModel(); // Handle potential null result

        }

        public async Task<IEnumerable<ReservationAssignmentListResponseModel>> ReservationAssignmentListSearch(ReservationAssignmentWebViewModel model)
        {
            string procedureName = ProcEntities.spReservationAssignmentsListSearch;

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

            if (model.ClaimantID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimantID, model.ClaimantID, DbType.Int32);

            if (model.ClaimID.ToValidateIntWithZero())
                parameters.Add(DbParams.ClaimID, model.ClaimID, DbType.Int32);

            if (model.CustomerID.ToValidateIntWithZero())
                parameters.Add(DbParams.CustomerID, model.CustomerID, DbType.Int32);

            if (!string.IsNullOrEmpty(model.ASSGNCode))
                parameters.Add(DbParams.ASSGNCode, model.ASSGNCode, DbType.String);

            if (!string.IsNullOrEmpty(model.RSVACCode))
                parameters.Add(DbParams.RSVACCode, model.RSVACCode, DbType.String);

            if (!string.IsNullOrEmpty(model.AssignmentJobStatus))
                parameters.Add(DbParams.AssignmentJobStatus, model.AssignmentJobStatus, DbType.String);

            if (model.ContractorContactInfo != null && model.ContractorContactInfo.Count > 0)
                parameters.Add(DbParams.ContractorContactInfo, string.Join(",", model.ContractorContactInfo), DbType.String);

            if (model.inactiveflag.ToValidateInActiveFlag())
                parameters.Add(DbParams.InactiveFlag, model.inactiveflag, DbType.Int32);

            if (model.ConAssignStatus.ToValidateInt())
                parameters.Add(DbParams.ConAssignStatus, model.ConAssignStatus, DbType.Int32);

            parameters.Add(DbParams.Page, model.Page, DbType.Int32);
            parameters.Add(DbParams.Limit, model.Limit, DbType.Int32);

            return await _context.ExecuteQueryAsync<ReservationAssignmentListResponseModel>(procedureName, parameters, CommandType.StoredProcedure);
        }
    }
}