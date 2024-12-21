namespace MembershipSite.Logic.Services;

using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Threading.Tasks;

public class MemberAdminService(MemberDal memberDal)
{
    public IQueryable<Member> AllAsQueryable()
    {
        return memberDal.AllAsQueryable();
    }

    public async Task<MemberCsvUploadResult> UploadMembersAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csv.GetRecords<MemberCsvRow>().ToList();
        var members = await memberDal.AllAsQueryable().ToListAsync();
        var rowIndex = 0;
        var result = new MemberCsvUploadResult();

        foreach (var record in records)
        {
            TrimFields(record);

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

    private static void TrimFields(MemberCsvRow record)
    {
        if (record is null)
        {
            return;
        }

        record.Email = record.Email.Trim();
        record.MemberNumber = record.MemberNumber.Trim();
        record.Name = record.Name.Trim();
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
