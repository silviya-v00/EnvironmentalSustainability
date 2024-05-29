using EnvironmentalSustainabilityApp.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Utils
{
    public class DBUtil
    {
        private string _connectionString;
        public DBUtil(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<CarbonFootprintTestState> GetCarbonFootprintTestState(string userID)
        {
            List<CarbonFootprintTestState> carbonFootprints = new List<CarbonFootprintTestState>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"
                                DECLARE @TotalCategoryCount int = (SELECT COUNT(*) FROM dbo.CarbonFootprintCategories)
                                DECLARE @CompletedCategoryCount int = (SELECT COUNT(*) FROM dbo.CarbonFootprintUser WHERE UserID = @UserID)
                                DECLARE @AllCategoriesCompleted bit = CASE WHEN @CompletedCategoryCount = @TotalCategoryCount THEN 1 ELSE 0 END

                                DECLARE @CategoriesNotCompleted TABLE (
                                    CarbonFootprintCategoryID int,
                                    CarbonFootprintCategoryKey nvarchar(max),
                                    CarbonFootprintCategoryName nvarchar(max)
                                )

                                IF @AllCategoriesCompleted = 0
                                BEGIN
                                    INSERT INTO @CategoriesNotCompleted (CarbonFootprintCategoryID, CarbonFootprintCategoryKey, CarbonFootprintCategoryName)
									SELECT c.CarbonFootprintCategoryID, c.CarbonFootprintCategoryKey, c.CarbonFootprintCategoryName
									FROM dbo.CarbonFootprintCategories c
									LEFT OUTER JOIN dbo.CarbonFootprintUser u ON c.CarbonFootprintCategoryID = u.CarbonFootprintCategoryID AND u.UserID = @UserID
									WHERE u.CarbonFootprintID IS NULL
                                END
																					
								SELECT
									@CompletedCategoryCount as CompletedCategoryCount,
									@TotalCategoryCount as TotalCategoryCount,
									a.CarbonFootprintCategoryID,
									a.CarbonFootprintCategoryKey,
									a.CarbonFootprintCategoryName
								FROM @CategoriesNotCompleted a";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    CarbonFootprintTestState testResult = new CarbonFootprintTestState();

                    if (dataReader["CompletedCategoryCount"] is int)
                        testResult.CompletedCategoryCount = (int)dataReader["CompletedCategoryCount"];

                    if (dataReader["TotalCategoryCount"] is int)
                        testResult.TotalCategoryCount = (int)dataReader["TotalCategoryCount"];

                    if (dataReader["CarbonFootprintCategoryID"] is int)
                        testResult.CarbonFootprintCategoryID = (int)dataReader["CarbonFootprintCategoryID"];

                    testResult.CarbonFootprintCategoryKey = dataReader["CarbonFootprintCategoryKey"].ToString();
                    testResult.CarbonFootprintCategoryName = dataReader["CarbonFootprintCategoryName"].ToString();

                    carbonFootprints.Add(testResult);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return carbonFootprints;
        }

        public List<CarbonFootprintChartData> GetCarbonFootprintChartData(string userID)
        {
            List<CarbonFootprintChartData> chartData = new List<CarbonFootprintChartData>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"
                                SELECT c.CarbonFootprintCategoryName as CategoryName,
	                                   u.CarbonFootprintResult as UserResult,
	                                   total.CarbonFootprintResult as TotalAvgResult,
	                                   ROW_NUMBER() OVER (ORDER BY (SELECT 1)) as Seq
                                FROM CarbonFootprintCategories c
                                LEFT OUTER JOIN CarbonFootprintUser u ON c.CarbonFootprintCategoryID = u.CarbonFootprintCategoryID AND u.UserID = @UserID
                                LEFT OUTER JOIN (SELECT CarbonFootprintCategoryID, AVG(CarbonFootprintResult) as CarbonFootprintResult
				                                 FROM CarbonFootprintUser
				                                 GROUP BY CarbonFootprintCategoryID) total ON c.CarbonFootprintCategoryID = total.CarbonFootprintCategoryID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    CarbonFootprintChartData row = new CarbonFootprintChartData();

                    row.CategoryName = dataReader["CategoryName"].ToString();

                    if (dataReader["UserResult"] is decimal)
                        row.UserResult = Math.Round((decimal)dataReader["UserResult"] / 1000, 3); // convert from kg to tons

                    if (dataReader["TotalAvgResult"] is decimal)
                        row.TotalAvgResult = Math.Round((decimal)dataReader["TotalAvgResult"] / 1000, 3); // convert from kg to tons

                    if (dataReader["Seq"] is int)
                        row.Seq = (int)dataReader["Seq"];

                    chartData.Add(row);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return chartData;
        }

        public decimal? GetCarbonFootprintByCategory(string userID, string categoryKey)
        {
            decimal? carbonFootprintResult = null;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"
                                SELECT a.CarbonFootprintResult
                                FROM dbo.CarbonFootprintUser a
                                INNER JOIN dbo.CarbonFootprintCategories b ON a.CarbonFootprintCategoryID = b.CarbonFootprintCategoryID
                                WHERE a.UserID = @UserID AND b.CarbonFootprintCategoryKey = @CategoryKey";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                command.Parameters.Add("@CategoryKey", System.Data.SqlDbType.NVarChar).Value = categoryKey;
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    if (dataReader["CarbonFootprintResult"] is decimal)
                        carbonFootprintResult = Math.Round((decimal)dataReader["CarbonFootprintResult"] / 1000, 3); // convert from kg to tons
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return carbonFootprintResult;
        }

        public void SaveCarbonFootprintByCategory(string userID, string categoryKey, decimal carbonFootprintResult)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"
								DECLARE @CategoryID int = (SELECT CarbonFootprintCategoryID FROM dbo.CarbonFootprintCategories WHERE CarbonFootprintCategoryKey = @CategoryKey)
								
								DELETE FROM dbo.CarbonFootprintUser
								WHERE UserID = @UserID AND CarbonFootprintCategoryID = @CategoryID

								INSERT INTO dbo.CarbonFootprintUser (UserID, CarbonFootprintCategoryID, CarbonFootprintResult)
								VALUES (@UserID, @CategoryID, @CarbonFootprintResult)";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                command.Parameters.Add("@CategoryKey", System.Data.SqlDbType.NVarChar).Value = categoryKey;
                command.Parameters.Add("@CarbonFootprintResult", System.Data.SqlDbType.Decimal).Value = carbonFootprintResult;
                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public FeaturedContent GetContentDetailsFromDatabase(int contentID)
        {
            FeaturedContent contentData = new FeaturedContent();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT ContentID, ContentTitle, ContentDescription, ContentLink, ContentImageFileName, IsContentActive
                               FROM dbo.FeaturedContent
                               WHERE ContentID = @ContentID
                               ORDER BY ContentTitle";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@ContentID", System.Data.SqlDbType.Int).Value = contentID;
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    contentData.ContentID = (int)dataReader["ContentID"];
                    contentData.ContentTitle = dataReader["ContentTitle"].ToString();
                    contentData.ContentDescription = dataReader["ContentDescription"].ToString();
                    contentData.ContentLink = dataReader["ContentLink"].ToString();
                    contentData.ContentImageFileName = dataReader["ContentImageFileName"].ToString();
                    contentData.IsContentActive = (bool)dataReader["IsContentActive"];
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return contentData;
        }

        public List<FeaturedContent> GetContentListFromDatabase(string contentType)
        {
            List<FeaturedContent> allContent = new List<FeaturedContent>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT ContentID, ContentTitle, ContentDescription, ContentLink, ContentImageFileName, IsContentActive
                               FROM dbo.FeaturedContent
                               WHERE @ContentType = 'ALL' OR (@ContentType = 'ACTIVE' AND IsContentActive = 1) OR (@ContentType = 'INACTIVE' AND IsContentActive = 0)
                               ORDER BY ContentTitle";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@ContentType", System.Data.SqlDbType.NVarChar).Value = contentType;
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    FeaturedContent contentData = new FeaturedContent();
                    contentData.ContentID = (int)dataReader["ContentID"];
                    contentData.ContentTitle = dataReader["ContentTitle"].ToString();
                    contentData.ContentDescription = dataReader["ContentDescription"].ToString();
                    contentData.ContentLink = dataReader["ContentLink"].ToString();
                    contentData.ContentImageFileName = dataReader["ContentImageFileName"].ToString();
                    contentData.IsContentActive = (bool)dataReader["IsContentActive"];

                    allContent.Add(contentData);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return allContent;
        }

        public void SaveContentToDatabase(FeaturedContent content, string imageFolderPath, string userID)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"DECLARE @OldContentImageFileName nvarchar(max),
                                       @NewContentImageFileName nvarchar(max)

                               IF ISNULL(@ContentID, '') <> ''
                               BEGIN
                                   SET @OldContentImageFileName = (SELECT ContentImageFileName
																   FROM dbo.FeaturedContent
																   WHERE ContentID = @ContentID)

	                               UPDATE dbo.FeaturedContent
	                               SET ContentTitle = @ContentTitle,
		                               ContentDescription = @ContentDescription,
		                               ContentLink = @ContentLink,
		                               ContentImageFileName = @ContentImageFileName,
		                               LastUpdatedBy = @LastUpdatedBy,
		                               IsContentActive = @IsContentActive
	                               WHERE ContentID = @ContentID

                                   SET @NewContentImageFileName = (SELECT ContentImageFileName 
																   FROM dbo.FeaturedContent
																   WHERE ContentID = @ContentID)
                               END
                               ELSE
                               BEGIN
	                               INSERT INTO dbo.FeaturedContent (ContentTitle, ContentDescription, ContentLink, ContentImageFileName, LastUpdatedBy, IsContentActive)
	                               VALUES (@ContentTitle, @ContentDescription, @ContentLink, @ContentImageFileName, @LastUpdatedBy, @IsContentActive)
                               END

							   SELECT @OldContentImageFileName as OldContentImageFileName, @NewContentImageFileName as NewContentImageFileName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);

                if (content.ContentID != 0)
                    command.Parameters.Add("@ContentID", System.Data.SqlDbType.Int).Value = content.ContentID;
                else
                    command.Parameters.Add("@ContentID", System.Data.SqlDbType.NVarChar).Value = DBNull.Value;

                if (!String.IsNullOrEmpty(content.ContentTitle))
                    command.Parameters.Add("@ContentTitle", System.Data.SqlDbType.NVarChar).Value = content.ContentTitle;
                else
                    command.Parameters.Add("@ContentTitle", System.Data.SqlDbType.NVarChar).Value = DBNull.Value;

                if (!String.IsNullOrEmpty(content.ContentDescription))
                    command.Parameters.Add("@ContentDescription", System.Data.SqlDbType.NVarChar).Value = content.ContentDescription;
                else
                    command.Parameters.Add("@ContentDescription", System.Data.SqlDbType.NVarChar).Value = DBNull.Value;

                if (!String.IsNullOrEmpty(content.ContentLink))
                    command.Parameters.Add("@ContentLink", System.Data.SqlDbType.NVarChar).Value = content.ContentLink;
                else
                    command.Parameters.Add("@ContentLink", System.Data.SqlDbType.NVarChar).Value = DBNull.Value;

                if (!String.IsNullOrEmpty(content.ContentImageFileName))
                    command.Parameters.Add("@ContentImageFileName", System.Data.SqlDbType.NVarChar).Value = content.ContentImageFileName;
                else if (!String.IsNullOrEmpty(content.ExistingContentImageFileName))
                    command.Parameters.Add("@ContentImageFileName", System.Data.SqlDbType.NVarChar).Value = content.ExistingContentImageFileName;
                else
                    command.Parameters.Add("@ContentImageFileName", System.Data.SqlDbType.NVarChar).Value = DBNull.Value;

                command.Parameters.Add("@IsContentActive", System.Data.SqlDbType.Bit).Value = content.IsContentActive;
                command.Parameters.Add("@LastUpdatedBy", System.Data.SqlDbType.NVarChar).Value = userID;

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    string oldImageFileName = dataReader["OldContentImageFileName"].ToString();
                    string newImageFileName = dataReader["NewContentImageFileName"].ToString();

                    if (oldImageFileName != newImageFileName)
                        DeleteImageFromFolder(oldImageFileName, imageFolderPath);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void DeleteContentFromDatabase(int contentID, string imageFolderPath)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT ContentImageFileName
                               FROM dbo.FeaturedContent
                               WHERE ContentID = @ContentID

                               DELETE FROM dbo.FeaturedContent
                               WHERE ContentID = @ContentID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@ContentID", System.Data.SqlDbType.Int).Value = contentID;
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    string imageFileName = dataReader["ContentImageFileName"].ToString();
                    DeleteImageFromFolder(imageFileName, imageFolderPath);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void DeleteImageFromFolder(string imageFileName, string imageFolderPath)
        {
            if (!String.IsNullOrEmpty(imageFileName))
            {
                string imagePath = Path.Combine(imageFolderPath, imageFileName);
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
        }
    }
}
