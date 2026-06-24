using System.Collections;

public interface ICoroutineRunner
{
    void RunCoroutine(IEnumerator routine);
}