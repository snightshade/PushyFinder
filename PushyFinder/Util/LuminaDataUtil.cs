using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace PushyFinder.Util;

public class LuminaDataUtil
{
    private readonly IDataManager dataManager;

    public LuminaDataUtil(IDataManager dataManager)
    {
        this.dataManager = dataManager;
    }

    public string GetJobAbbreviation(uint jobId)
    {
        var jobEnum = dataManager.GetExcelSheet<ClassJob>()
                             .Where(a => a.RowId == jobId);
        var job = jobEnum.FirstOrDefault();
        return job.Abbreviation.ToString();
    }
}