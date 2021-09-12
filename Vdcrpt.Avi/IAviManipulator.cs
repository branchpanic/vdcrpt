namespace Vdcrpt.Avi
{
    public interface IAviManipulator
    {
        int FrameCount { get; }

        delegate void FrameAction(FrameMetadata metadata, ref byte[] data);
        void ForEachFrame(FrameAction action);
    }
}