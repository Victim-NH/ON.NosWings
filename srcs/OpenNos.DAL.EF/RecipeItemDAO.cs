﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF.Base;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Entities;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class RecipeItemDAO : MappingBaseDao<RecipeItem, RecipeItemDTO>, IRecipeItemDAO
    {
        #region Methods

        public RecipeItemDTO Insert(RecipeItemDTO recipeItem)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    var entity = _mapper.Map<RecipeItem>(recipeItem);
                    context.RecipeItem.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<RecipeItemDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<RecipeItemDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (RecipeItem recipeItem in context.RecipeItem)
                {
                    yield return _mapper.Map<RecipeItemDTO>(recipeItem);
                }
            }
        }

        public RecipeItemDTO LoadById(short recipeItemId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<RecipeItemDTO>(context.RecipeItem.FirstOrDefault(s => s.RecipeItemId.Equals(recipeItemId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<RecipeItemDTO> LoadByRecipe(short recipeId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (RecipeItem recipeItem in context.RecipeItem.Where(s => s.RecipeId.Equals(recipeId)))
                {
                    yield return _mapper.Map<RecipeItemDTO>(recipeItem);
                }
            }
        }

        public IEnumerable<RecipeItemDTO> LoadByRecipeAndItem(short recipeId, short itemVNum)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (RecipeItem recipeItem in context.RecipeItem.Where(s => s.ItemVNum.Equals(itemVNum) && s.RecipeId.Equals(recipeId)))
                {
                    yield return _mapper.Map<RecipeItemDTO>(recipeItem);
                }
            }
        }

        #endregion
    }
}