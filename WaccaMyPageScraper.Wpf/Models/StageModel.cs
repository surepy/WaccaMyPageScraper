﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaccaMyPageScraper.Data;
using WaccaMyPageScraper.Enums;
using WaccaMyPageScraper.Resources;
using WaccaMyPageScraper.Wpf.Resources;

namespace WaccaMyPageScraper.Wpf.Models
{
    public class StageModel : StageDetail
    {
        public byte[] StageIcon => ImageLocator.LocateStage(this);

        public StageData StageData => StageData.Read()[this.Id];

        public StageModel() { }

        public StageModel(StageDetail stage) : base(stage.Id, stage.Name, stage.Grade, stage.Scores) { }

        public static StageModel FromStageDetail(StageDetail data) => new StageModel(data);

        public static IEnumerable<StageModel> FromStageDetails(IEnumerable<StageDetail> data)
        {
            if (data is null)
                return null;

            var stages = new List<StageModel>();
            foreach (var stageDetail in data)
                stages.Add(FromStageDetail(stageDetail));

            return stages;
        }
    }

    public class StageData
    {
        [JsonProperty("tracks")]
        public StageTrack[] Tracks { get; set; }

        public byte[][] StageTrackImages
        {
            get
            {
                var images = new byte[3][];

                for (int i = 0; i < 3; i++)
                {
                    var musicId = this.Tracks[i].Id;
                    var filePath = Path.Combine(DataFilePath.RecordImage, musicId + ".png");

                    if (!File.Exists(filePath))
                        return null;

                    byte[] image = null;
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        image = new byte[fs.Length];
                        while (fs.Read(image, 0, image.Length) > 0)
                        {
                            image[image.Length - 1] = (byte)fs.ReadByte();
                        }
                    }

                    images[i] = image;
                }

                return images;
            }
        }
        
        [JsonProperty("clearCondition")]
        public StageCondition ClearCondition { get; set; }
        
        [JsonProperty("lifeRestore")]
        public int LifeRestore { get; set; }

        public string LifeRestoreText => this.LifeRestore == 0 ? 
            string.Empty : $"Restore {this.LifeRestore} Lifes";

        public StageData() { }

        public StageData(StageTrack[] tracks, StageCondition clearCondition, int lifeRestore)
        {
            this.Tracks = tracks;
            this.ClearCondition = clearCondition;
            this.LifeRestore = lifeRestore;
        }

        public static IDictionary<int, StageData> Read()
            => JsonConvert.DeserializeObject<IDictionary<int, StageData>>(Properties.Resources.StageUpJson.ToString());
    }

    public struct StageTrack
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        public string LevelText => string.Format("{0} {1}", 
            this.Difficulty.ToString().ToUpperInvariant(), this.Level);

        [JsonProperty("difficulty")]
        public Difficulty Difficulty { get; set; }

        public string DifficultyColor => this.Difficulty switch
        {
            Difficulty.Normal => "#009DE6",
            Difficulty.Hard => "#FED131",
            Difficulty.Expert => "#FC06A3",
            Difficulty.Inferno => "#4A004F"
        };
        
        public StageTrack(int id, string title, string level, Difficulty difficulty)
        {
            this.Id = id;
            this.Title = title;
            this.Level = level;
            this.Difficulty = difficulty;
        }
    }

    public struct StageCondition
    {
        [JsonProperty("judge")]
        public string Judge { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        public StageCondition(string judge, int count)
        {
            this.Judge = judge;
            this.Count = count;
        }

        public override string ToString() => $"In {this.Count} {this.Judge}s";
    }
}
