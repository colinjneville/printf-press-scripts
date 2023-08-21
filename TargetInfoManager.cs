using Functional.Option;
using Newtonsoft.Json;
using IntervalTree;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

partial class ExecutionContextFullFirst {
    public sealed class TargetInfoManager : IDisposable {
        private Workspace workspace;
        private Dictionary<TargetInfo, List<ArrowInfo>> arrowLookup;
        private int lookaheadCount;
        private bool disposed;

        public TargetInfoManager(Workspace workspace, int lookaheadCount) {
            this.workspace = workspace;
            arrowLookup = new Dictionary<TargetInfo, List<ArrowInfo>>();
            this.lookaheadCount = lookaheadCount;
        }

        public void Dispose() {
            if (!disposed) {
                foreach (var arrows in arrowLookup.Values) {
                    foreach (var arrow in arrows) {
                        RemoveArrowInfo(arrow);
                    }
                }
                disposed = true;
                arrowLookup.Clear();
            }
        }

        private void AddArrowInfo(ArrowInfo arrow) {
            Assert.False(disposed);
            foreach (var cryptexView in workspace.GetCryptex(arrow.TargetInfo.Roller.Cryptex.Id).ValueOrAssert().View) {
                cryptexView.AddArrow(arrow);
            }
        }

        private void RemoveArrowInfo(ArrowInfo arrow) {
            if (!disposed) {
                // We may be updating because the cryptex has been removed, so skip missing cryptexes
                foreach (var cryptex in workspace.GetCryptex(arrow.TargetInfo.Roller.Cryptex.Id)) {
                    foreach (var cryptexView in cryptex.View) {
                        cryptexView.RemoveArrow(arrow);
                    }
                }
            }
        }

        public void AddTargetInfos(IEnumerable<TargetInfo> targetInfos, int creationTime, int currentTime) {
            var changedCryptexes = new HashSet<Cryptex>();

            foreach (var targetInfo in targetInfos) {
                var arrow = new ArrowInfo(targetInfo, creationTime);
                AddArrowInfo(arrow);
                List<ArrowInfo> list;
                if (!arrowLookup.TryGetValue(targetInfo, out list)) {
                    list = new List<ArrowInfo>();
                    arrowLookup.Add(targetInfo, list);
                }
                list.Add(arrow);
                changedCryptexes.Add(workspace.GetCryptex(targetInfo.Roller.Cryptex.Id).ValueOrAssert());
            }

            // Since we added/removed arrows, we must be advancing the clock, and therefore Updating anyway. No need to do this (again) here
            //foreach (var cryptex in changedCryptexes) {
            //    foreach (var view in cryptex.View) {
            //        view.UpdateArrows(currentTime, lookaheadCount);
            //    }
            //}
        }

        public void RemoveTargetInfos(IEnumerable<TargetInfo> targetInfos, int creationTime, int currentTime) {
            var changedCryptexes = new HashSet<Cryptex>();

            foreach (var targetInfo in targetInfos) {
                // TEMP
                if (!arrowLookup.ContainsKey(targetInfo)) {
                    Debug.Log($"Could not find: {targetInfo}");
                    foreach (var ti in arrowLookup) {
                        Debug.Log($"{ti.Key}");
                    }
                    Debug.Log("---");
                }

                var list = arrowLookup[targetInfo];
                bool found = false;
                for (int i = 0; i < list.Count; ++i) {
                    if (list[i].CreationTime == creationTime) {
                        var arrow = list[i];
                        list.RemoveAt(i);
                        RemoveArrowInfo(arrow);
                        if (list.Count == 0) {
                            arrowLookup.Remove(targetInfo);
                        }
                        found = true;
                        break;
                    }
                }
                Assert.True(found);

                changedCryptexes.Add(workspace.GetCryptex(targetInfo.Roller.Cryptex.Id).ValueOrAssert());
            }

            // Since we added/removed arrows, we must be advancing the clock, and therefore Updating anyway. No need to do this (again) here
            //foreach (var cryptex in changedCryptexes) {
            //    foreach (var view in cryptex.View) {
            //        view.UpdateArrows(currentTime, lookaheadCount);
            //    }
            //}
        }

        public void UpdateArrows(IEnumerable<Cryptex> cryptexes, int currentTime) {
            foreach (var cryptex in cryptexes) {
                foreach (var view in cryptex.View) {
                    view.UpdateArrows(currentTime, lookaheadCount);
                }
            }
        }
    }
}
