namespace Sels.Core.Testing.Models
{
    /// <summary>
    /// Simple model for testing serialization.
    /// </summary>
    public class ExamResult
    {
        /// <summary>
        /// Name of the student who took the exam.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Family name of the student who took the exam.
        /// </summary>
        public string FamilyName { get; set; }
        /// <summary>
        /// What course the exam was for.
        /// </summary>
        public string Course { get; set; }
        /// <summary>
        /// When the exam was taken.
        /// </summary>
        public DateTime ExecutionDate { get; set; }
        /// <summary>
        /// When the exam result was published.
        /// </summary>
        public DateTime? ResultDate { get; set; }
        /// <summary>
        /// The score of the student for this exam.
        /// </summary>
        public double Score { get; set; }
        /// <summary>
        /// Which professors signed this result.
        /// </summary>
        public List<string> Signatures { get; set; }
    }
}
