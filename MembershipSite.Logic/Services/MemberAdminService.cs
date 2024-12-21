namespace MembershipSite.Logic.Services;

using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Threading.Tasks;

public class MemberAdminService(ILogger<MemberAdminService> logger, MemberDal memberDal)
{
    public IQueryable<Member> AllAsQueryable()
    {
        return memberDal.AllAsQueryable();
    }

    public async Task<MemberCsvUploadResult> UploadMembersAsync(IFormFile file)
    {
        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                PrepareHeaderForMatch = (args) => args.Header.Trim().ToLower(),
                TrimOptions = TrimOptions.Trim,
            };
            using var csvReader = new CsvReader(reader, csvConfig);
            var records = csvReader.GetRecords<MemberCsvRow>().ToList();

            var result = await ProcessRecords(records);

            return Summarise(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading members.");

            var summary = $"Unable to upload the file you supplied. Support has been emailed. In case it helps, the error is below.";

            return new MemberCsvUploadResult { 
                Error = ex.Message,
                Summary = summary
            };
        }
    }

    private static MemberCsvUploadResult Summarise(MemberCsvUploadResult result)
    {
        if (result.Detail.Count == 0)
        {
            result.Summary = "No records processed.";
        }
        else if (result.Failed.Count == 0)
        {
            result.Summary = "All records processed successfully.";
        }
        else
        {
            result.Summary = $"{result.Failed.Count} records failed to process. Please check the details.";
        }

        return result;
    }

    private async Task<MemberCsvUploadResult> ProcessRecords(List<MemberCsvRow> records)
    {
        var members = await memberDal.AllAsQueryable().ToListAsync();
        var rowIndex = 0;
        var result = new MemberCsvUploadResult();

        foreach (var record in records)
        {
            var errors = ValidateCsvRecord(record, rowIndex);

            if (errors.Count > 0)
            {
                result.Failed.AddRange(errors);
                result.Detail.AddRange(errors);
                continue;
            }

            var existingMember = members
                .Where(m => string.Equals(m.MemberNumber, record.MemberNumber, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            if (existingMember is null)
            {
                existingMember = memberDal.Add(record.MemberNumber);
                existingMember.IsAdmin = false;
                existingMember.DateRegistered = DateTimeOffset.Now;

                var logLine = $"Member number {existingMember.MemberNumber} added.";
                result.Added.Add(logLine);
                result.Detail.Add(logLine);
            }
            else
            {
                var logLine = $"Member number {existingMember.MemberNumber} updated.";
                result.Updated.Add(logLine);
                result.Detail.Add(logLine);
            }

            existingMember.Email = record.Email;
            existingMember.Name = record.Name;
            existingMember.IsApproved = true;
        }

        await memberDal.CommitAsync();
        return result;
    }

    private static List<string> ValidateCsvRecord(MemberCsvRow record, int rowIndex)
    {
        if (record is null)
        {
            return [$"Row {rowIndex} blank and was skipped."];
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(record.Email))
        {
            errors.Add($"Row {rowIndex} missing email. Data was '{record}'.");
        }

        if (string.IsNullOrWhiteSpace(record.MemberNumber))
        {
            errors.Add($"Row {rowIndex} missing member number. Data was '{{record}}'.");
        }

        if (string.IsNullOrWhiteSpace(record.Name))
        {
            errors.Add($"Row {rowIndex} missing name. Data was '{{record}}'.");
        }

        if (record.Email.Length > MemberFieldLimits.Email)
        {
            errors.Add($"Row {rowIndex} email too long at {record.Email.Length} characters (maximum {MemberFieldLimits.Email}). Data was '{record}'.");
        }

        if (record.Name.Length > MemberFieldLimits.Name)
        {
            errors.Add($"Row {rowIndex} name too long at {record.Name.Length} characters (maximum {MemberFieldLimits.Name}). Data was '{record}'.");
        }

        if (record.MemberNumber.Length > MemberFieldLimits.MemberNumber)
        {
            errors.Add($"Row {rowIndex} member number too long at {record.MemberNumber.Length} characters (maximum {MemberFieldLimits.MemberNumber}). Data was '{record}'.");
        }

        return errors;
    }
}
