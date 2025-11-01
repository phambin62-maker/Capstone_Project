using BE_Capstone_Project.Application.Categories.DTOs;
using BE_Capstone_Project.Application.Categories.Services.Interfaces;
using BE_Capstone_Project.Application.Locations.Services.Interfaces;
using BE_Capstone_Project.DAO;
using BE_Capstone_Project.Domain.Models;

namespace BE_Capstone_Project.Application.Categories.Services
{
    public class TourCategoryService : ITourCategoryService
    {
        private readonly TourCategoryDAO _tourCategoryDAO;

        public TourCategoryService(TourCategoryDAO tourCategoryDAO)
        {
            _tourCategoryDAO = tourCategoryDAO;
        }

        public async Task<ServiceResult<TourCategoryDTO>> GetTourCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _tourCategoryDAO.GetTourCategoryById(id);
                if (category == null)
                {
                    return new ServiceResult<TourCategoryDTO>
                    {
                        Success = false,
                        Message = "Tour category not found"
                    };
                }

                var categoryDto = new TourCategoryDTO
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName
                };

                return new ServiceResult<TourCategoryDTO>
                {
                    Success = true,
                    Data = categoryDto
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<TourCategoryDTO>
                {
                    Success = false,
                    Message = $"Error retrieving tour category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<List<TourCategoryDTO>>> GetAllTourCategoriesAsync()
        {
            try
            {
                var categories = await _tourCategoryDAO.GetAllTourCategories();
                var categoryDtos = categories.Select(c => new TourCategoryDTO
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName
                }).ToList();

                return new ServiceResult<List<TourCategoryDTO>>
                {
                    Success = true,
                    Data = categoryDtos
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<TourCategoryDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving tour categories: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<int>> CreateTourCategoryAsync(CreateTourCategoryDTO createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.CategoryName))
                {
                    return new ServiceResult<int>
                    {
                        Success = false,
                        Message = "Category name is required"
                    };
                }

                var category = new TourCategory
                {
                    CategoryName = createDto.CategoryName
                };

                var categoryId = await _tourCategoryDAO.AddTourCategory(category);

                if (categoryId > 0)
                {
                    return new ServiceResult<int>
                    {
                        Success = true,
                        Data = categoryId,
                        Message = "Tour category created successfully"
                    };
                }
                else
                {
                    return new ServiceResult<int>
                    {
                        Success = false,
                        Message = "Failed to create tour category"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = $"Error creating tour category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> UpdateTourCategoryAsync(int id, UpdateTourCategoryDTO updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateDto.CategoryName))
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Category name is required"
                    };
                }

                var existingCategory = await _tourCategoryDAO.GetTourCategoryById(id);
                if (existingCategory == null)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Tour category not found"
                    };
                }

                existingCategory.CategoryName = updateDto.CategoryName;
                var result = await _tourCategoryDAO.UpdateTourCategory(existingCategory);

                return new ServiceResult<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Tour category updated successfully" : "Failed to update tour category"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"Error updating tour category: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> DeleteTourCategoryAsync(int id)
        {
            try
            {
                var result = await _tourCategoryDAO.DeleteTourCategoryById(id);
                return new ServiceResult<bool>
                {
                    Success = result,
                    Data = result,
                    Message = result ? "Tour category deleted successfully" : "Tour category not found or could not be deleted"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"Error deleting tour category: {ex.Message}"
                };
            }
        }
    }
}
