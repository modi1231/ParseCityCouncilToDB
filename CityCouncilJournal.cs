using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParseCityCouncilToDB
{
    class CityCouncilJournal
    {
        public string FileName;
        public string EntireFile;

        public string Tile;
        public string Date;
        public string SessionInfo;
        public string Roll;
        public bool Pledge;
        public string Invocation;
        public string CertofPubl;
        public List<string> PursuantTo;

        public string ConsetAgenda;
        public string Adjourned;

        public string WrapUp;

        public List<GeneralSection> genSections;

        
        public CityCouncilJournal()
        {
            genSections = new List<GeneralSection>();
            PursuantTo = new List<string>();
        }
    }

    class GeneralSection
    {
        public string SectionName;
        public string Information;

        public GeneralSection()
        {
            SectionName = string.Empty;
            Information = string.Empty;
        }

        public GeneralSection(string sect, string info)
        {
            SectionName = sect;
            Information = info;
        }
    }

}
