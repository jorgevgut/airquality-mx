using System.Collections.Generic;
using System;
using Amazon.DynamoDBv2.DocumentModel;
using Latincoder.AirQuality.Model.DTO;
using Latincoder.AirQuality.Model.Dynamo;

namespace Latincoder.AirQuality.Services
{
    /// <summary>
    /// The "Notifier" Service is in charge of determining when a Feed
    /// needs to be notified based on a set of and custom established rules
    /// </summary>
    public class NotificationValidator
    {

        // TODO: create custom criteria for specific use cases: ex. a "MeetsTwitterCriteria"
        // would need support to add custom rules and criterias

        private List<Predicate<CityFeed>> _globalRules;
        private NotificationValidator() {
            _globalRules = new List<Predicate<CityFeed>>();
        }

        public void AddRule(Predicate<CityFeed> rule) {
            _globalRules.Add(rule);
        }

        public void AddRules(params Predicate<CityFeed>[] rules) {
            _globalRules.AddRange(rules);
        }

        /// <summary>
        /// Meets global criteria is a criteria that must be met in order to notify
        /// no matter delivery channel
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public bool MeetsGlobalCriteria(CityFeed feed) {
            bool isValid = true; // default to "valid" as if no rules are applied
            _globalRules.ForEach(rule => isValid &= rule.Invoke(feed));
            return isValid;
        }

        /// <summary>
        /// Only considers valid notifications from las 10 minutes
        /// with an AQI Index of 51 or more
        /// </summary>
        /// <returns></returns>
        public static NotificationValidator CreateDefaultValidator() {
            var validator = new NotificationValidator();
            validator.AddRule(feed => feed.MaxAQI >= 51);
            // within last 10 minutes
            validator.AddRule(feed => feed.UpdatedAt.AddMinutes(10).CompareTo(DateTime.Now) > 0);
            return validator;
        }

        public static NotificationValidator CreateyValidator() {
            return new NotificationValidator();
        }

    }

        /// <summary>
        /// A set of helper functions to create rules applied to notification validation
        /// </summary>
    public static class Rules {
        public static bool FeedWithinLast10Minutes(CityFeed feed) {
            return feed.UpdatedAt.AddMinutes(10).CompareTo(DateTime.Now) > 0;
        }

        public static Predicate<CityFeed> FeedWithinLastMinutes(double minutes) {
            return feed => feed.UpdatedAt.AddMinutes(minutes).CompareTo(DateTime.Now) > 0;
        }

        public static Predicate<CityFeed> DeltaShowsAqiIsHigher(Document feedDoc) {
            var feed = CityFeedDocument.ToDTO(feedDoc);
            if (feedDoc.ContainsKey(CityFeedDocument.FieldDeltaAqi)) {
                return feed => feedDoc[CityFeedDocument.FieldDeltaAqi].AsInt() > 0;
            }
            return (feed) => true;
        }

        public static bool AqiAboveModerateThreshold(CityFeed feed) {
            return feed.MaxAQI >= 51;
        }

        public static bool AqiAboveUnhealthyForSensitiveGroupsThreshold(CityFeed feed) {
            return feed.MaxAQI >= 101;
        }

        public static bool AqiAboveUnhealthyThreshold(CityFeed feed) {
            return feed.MaxAQI >= 151;
        }

        public static bool AqiAboveHazardousThreshold(CityFeed feed) {
            return feed.MaxAQI >= 151;
        }

        public static Predicate<CityFeed> AbsoluteAqiChangedBy(CityFeed prevFeed, int absoluteDelta) {
            return feed => {
                    if (prevFeed == null) { return true; }
                    return Math.Abs(prevFeed.MaxAQI - feed.MaxAQI) > absoluteDelta;
            };
        }

        public static Predicate<CityFeed> MinutesApartFrom(CityFeed prevFeed, int minutes) {
            return feed => {
                    if (prevFeed == null) { return true; }
                    return prevFeed.UpdatedAt.AddMinutes(minutes).CompareTo(feed) < 0;
                };
        }

        public static Predicate<CityFeed> AqiBecameGoodFromModerateOrWorseWithDelta(CityFeed feed, int delta) {
            return feed => feed.MaxAQI <= 50 && delta < 0;
        }

        public static bool AqiBecameGoodFromModerateOrWorse(Document feedDoc) {
            if (feedDoc.ContainsKey(CityFeedDocument.FieldDeltaAqi) &&
                feedDoc.ContainsKey(CityFeedDocument.FieldMaxAqi)) {
                return feedDoc[CityFeedDocument.FieldDeltaAqi].AsInt() < 0 &&
                        feedDoc[CityFeedDocument.FieldMaxAqi].AsInt() <= 50;
            }
            return false; // defaults to false if delta is unknown
        }
    }
}
