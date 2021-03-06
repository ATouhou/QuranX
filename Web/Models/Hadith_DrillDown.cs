﻿using RationalizingIslam.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuranX.Models
{
	public class Hadith_DrillDown
	{
		public HadithCollection Collection { get; private set; }
		public List<KeyValuePair<string, string>> SelectedKeyParts { get; private set; }
		public string NextKeyPartName { get; private set; }
		public IEnumerable<KeyValuePair<HadithReference,Hadith>> HadithsInCurrentSelection { get; private set; }
		public IEnumerable<string> NextKeyPartSelection { get; private set; }
        public HadithReferenceDefinition ReferenceDefinition { get; private set; }

		public Hadith_DrillDown(
			string collectionCode,
            string indexCode,
			string path)
		{
			path = path ?? "";
			this.Collection = SharedData.Document.HadithDocument[collectionCode];
            if (!string.IsNullOrWhiteSpace(indexCode))
            {
                ReferenceDefinition = Collection.GetReferenceDefinition(indexCode);
                if (ReferenceDefinition == null)
                    throw new KeyNotFoundException(nameof(indexCode));
            }
            else
                ReferenceDefinition = Collection.PrimaryReferenceDefinition;

			this.NextKeyPartSelection = new List<string>();
			this.SelectedKeyParts =
				path.Split('/')
				.Where(x => !string.IsNullOrEmpty(x))
				.Select(x => x.Trim())
				.Where(x => x != "")
				.Select(x =>
					{
						string[] keyAndValue = x.Split('-');
						return new KeyValuePair<string, string>(keyAndValue[0], keyAndValue[1]);
					}
				)
				.ToList();

			int referencePartIndex = 0;
            HadithsInCurrentSelection =
                Collection
                .Hadiths
                .Select(x => new KeyValuePair<HadithReference, Hadith>(x.GetReference(ReferenceDefinition.Code), x))
                .Where(x => x.Key != null)
                .OrderBy(x => x.Key);
			foreach (var keyPartAndValue in SelectedKeyParts)
			{
				if (string.Compare(keyPartAndValue.Key, ReferenceDefinition.PartNames[referencePartIndex], true) != 0)
					throw new ArgumentException(string.Format("Expected key part {0} but found {1}", ReferenceDefinition.PartNames[referencePartIndex], keyPartAndValue.Key));

				HadithsInCurrentSelection = 
                    HadithsInCurrentSelection
                    .Where(x => string.Compare(x.Key[referencePartIndex], keyPartAndValue.Value, true) == 0)
					.ToList();
				referencePartIndex++;
			}
			if (referencePartIndex < ReferenceDefinition.PartNames.Length)
			{
				NextKeyPartName = ReferenceDefinition.PartNames[referencePartIndex];
				NextKeyPartSelection = HadithsInCurrentSelection.Select(x => x.Key[referencePartIndex]).Distinct();
			}
		}

	}
}