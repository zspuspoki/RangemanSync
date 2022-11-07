namespace Rangeman.WatchDataSender
{
    internal class CategoryToSend
    {
        public CategoryToSend(byte categoryId, byte[] data)
        {
            CategoryId = categoryId;
            Data = data;
        }

        public byte CategoryId { get; }
        public byte[] Data { get; }
    }
}