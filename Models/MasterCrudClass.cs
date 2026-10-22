using SupermarketRepository;

namespace MasterClass.Models
{
    public class MasterCrudClass :DBClass
    {
        private readonly MyDbOptions _options;
       public MasterCrudClass(MyDbOptions options):base(options)
        {
            _options = options;
         
        }
    }
}
