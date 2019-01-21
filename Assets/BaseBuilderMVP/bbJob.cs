using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBMVP
{

    public interface bbJob
    {
        void doJobTurn(bbAgent agent);
        bool checkJobComplete(bbAgent agent);
    }

    public class bbJobMoveTo : bbJob
    {
        bbPos target;
        List<bbPos> path;
        public bbJobMoveTo(bbPos _target)
        {
            target = _target;
        }
        public void doJobTurn(bbAgent _agent)
        {
            path = _agent.pos.findPath(target);
            if (path != null && path.Count != 0)
            {
                if (_agent.pos == path[0])
                {
                    path.Remove(_agent.pos);
                }
                _agent.lastPos = _agent.pos;
                _agent.pos = path[0];
            }
        }
        public bool checkJobComplete(bbAgent _agent)
        {
            if (_agent.pos == target)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class bbJobUseStructure : bbJob
    {
        bbStructure structure;
        bool jobComplete;
        public bbJobUseStructure(bbStructure _structure)
        {
            structure = _structure;
            jobComplete = false;
        }
        public bool checkJobComplete(bbAgent agent)
        {
            return jobComplete;
        }

        public void doJobTurn(bbAgent agent)
        {
            if (structure.getPos() == agent.pos)
            {
                jobComplete = structure.UseStructure(agent);
            }
            else
            {
                bbJobMoveTo move = new bbJobMoveTo(structure.getPos());
                agent.jobQueue.Insert(0, move);
            }
        }
    }
}

