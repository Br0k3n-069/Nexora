using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using NEXORA.Models;

namespace NEXORA.Services
{
    public sealed class ProfileStore
    {
        private readonly string _filePath;

        public ProfileStore()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NEXORA");
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "profiles.json");
        }

        public ObservableCollection<Profile> Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new ObservableCollection<Profile>();

                using (var stream = File.OpenRead(_filePath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ProfileDto[]));
                    var rows = (ProfileDto[])serializer.ReadObject(stream);
                    return new ObservableCollection<Profile>((rows ?? new ProfileDto[0])
                        .Select(x => new Profile { Name = x.Name, Address = x.Address, CreatedAt = x.CreatedAt }));
                }
            }
            catch
            {
                // Keep the app usable if the profile file is damaged.
                return new ObservableCollection<Profile>();
            }
        }

        public void Save(ObservableCollection<Profile> profiles)
        {
            var rows = profiles.Select(x => new ProfileDto
            {
                Name = x.Name,
                Address = x.Address,
                CreatedAt = x.CreatedAt
            }).ToArray();

            var serializer = new DataContractJsonSerializer(typeof(ProfileDto[]));
            var temp = _filePath + ".tmp";
            using (var stream = File.Create(temp))
                serializer.WriteObject(stream, rows);

            if (File.Exists(_filePath)) File.Delete(_filePath);
            File.Move(temp, _filePath);
        }

        [DataContract]
        private class ProfileDto
        {
            [DataMember] public string Name { get; set; }
            [DataMember] public string Address { get; set; }
            [DataMember] public DateTime CreatedAt { get; set; }
        }
    }
}
