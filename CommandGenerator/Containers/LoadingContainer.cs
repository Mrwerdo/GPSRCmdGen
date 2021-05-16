using System.Collections.Generic;

namespace RoboCup.AtHome.CommandGenerator.Containers {
    public interface ILoadingContainer<Result> {
        List<Result> Results { get; }
    }
}