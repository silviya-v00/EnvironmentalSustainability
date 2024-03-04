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

        public List<CarbonFootprintTestResult> GetCarbonFootprintResult(string userID)
        {
            List<CarbonFootprintTestResult> carbonFootprints = new List<CarbonFootprintTestResult>();
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
                                    CarbonFootprintCategoryName nvarchar(max)
                                )

                                IF @AllCategoriesCompleted = 0
                                BEGIN
                                    INSERT INTO @CategoriesNotCompleted (CarbonFootprintCategoryID, CarbonFootprintCategoryName)
                                    SELECT c.CarbonFootprintCategoryID, c.CarbonFootprintCategoryName
                                    FROM dbo.CarbonFootprintCategories c
                                    LEFT OUTER JOIN dbo.CarbonFootprintUser u ON c.CarbonFootprintCategoryID = u.CarbonFootprintCategoryID
                                    WHERE u.CarbonFootprintID IS NULL
                                END

                                SELECT
                                    @CompletedCategoryCount as CompletedCategoryCount,
	                                @TotalCategoryCount as TotalCategoryCount,
                                    a.CarbonFootprintCategoryID,
                                    a.CarbonFootprintCategoryName,
	                                b.CarbonFootprintResult
                                FROM @CategoriesNotCompleted a
                                LEFT OUTER JOIN dbo.CarbonFootprintUser b on a.CarbonFootprintCategoryID = b.CarbonFootprintCategoryID and b.UserID = @UserID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    CarbonFootprintTestResult testResult = new CarbonFootprintTestResult();

                    if (dataReader["CompletedCategoryCount"] is int)
                        testResult.CompletedCategoryCount = (int)dataReader["CompletedCategoryCount"];

                    if (dataReader["TotalCategoryCount"] is int)
                        testResult.TotalCategoryCount = (int)dataReader["TotalCategoryCount"];

                    if (dataReader["CarbonFootprintCategoryID"] is int)
                        testResult.CarbonFootprintCategoryID = (int)dataReader["CarbonFootprintCategoryID"];

                    testResult.CarbonFootprintCategoryName = dataReader["CarbonFootprintCategoryName"].ToString();

                    if (dataReader["CarbonFootprintResult"] is decimal)
                        testResult.CarbonFootprintResult = (decimal)dataReader["CarbonFootprintResult"];

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

        public void SaveContentToDatabase(FeaturedContent content, string imageFolderPath)
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
		                               IsContentActive = @IsContentActive
	                               WHERE ContentID = @ContentID

                                   SET @NewContentImageFileName = (SELECT ContentImageFileName 
																   FROM dbo.FeaturedContent
																   WHERE ContentID = @ContentID)
                               END
                               ELSE
                               BEGIN
	                               INSERT INTO dbo.FeaturedContent (ContentTitle, ContentDescription, ContentLink, ContentImageFileName, IsContentActive)
	                               VALUES (@ContentTitle, @ContentDescription, @ContentLink, @ContentImageFileName, @IsContentActive)
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
