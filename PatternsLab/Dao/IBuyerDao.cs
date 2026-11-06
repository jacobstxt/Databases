namespace PatternsLab.Dao;

public interface IBuyerDao
{
    int Insert(BuyerModel buyer);    
    BuyerModel? GetById(int id);              
    List<BuyerModel> GetAll(int? limit = null);
    bool Update(BuyerModel buyer);         
    bool Delete(int id);                     
}